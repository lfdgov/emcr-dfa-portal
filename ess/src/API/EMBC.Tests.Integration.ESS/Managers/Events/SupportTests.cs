﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EMBC.ESS.Managers.Events;
using EMBC.ESS.Shared.Contracts;
using EMBC.ESS.Shared.Contracts.Events;
using EMBC.ESS.Utilities.Dynamics;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace EMBC.Tests.Integration.ESS.Managers.Events
{
    public class SupportTests : DynamicsWebAppTestBase
    {
        private readonly EventsManager manager;

        private async Task<RegistrantProfile> GetRegistrantByUserId(string userId) => (await TestHelper.GetRegistrantByUserId(manager, userId)).ShouldNotBeNull();

        private async Task<IEnumerable<EvacuationFile>> GetEvacuationFileById(string fileId) => await TestHelper.GetEvacuationFileById(manager, fileId);

        private EvacuationFile CreateNewTestEvacuationFile(RegistrantProfile registrant) => TestHelper.CreateNewTestEvacuationFile(registrant);

        public SupportTests(ITestOutputHelper output, DynamicsWebAppFixture fixture) : base(output, fixture)
        {
            manager = Services.GetRequiredService<EventsManager>();
        }

        [Fact(Skip = RequiresVpnConnectivity)]
        public async Task CanProcessSupports()
        {
            var registrant = await GetRegistrantByUserId(TestData.ContactUserId);
            var file = CreateNewTestEvacuationFile(registrant);

            file.NeedsAssessment.CompletedOn = DateTime.UtcNow;
            file.NeedsAssessment.CompletedBy = new TeamMember { Id = TestData.Tier4TeamMemberId };

            var fileId = await manager.Handle(new SubmitEvacuationFileCommand { File = file });

            var supports = new Support[]
            {
                new ClothingSupport { TotalAmount = 100, SupportDelivery = new Interac { NotificationEmail = "test@test.com", ReceivingRegistrantId = registrant.Id } },
                new IncidentalsSupport { TotalAmount = 100, SupportDelivery = new Interac { NotificationEmail = "test@test.com", ReceivingRegistrantId = registrant.Id } },
                new FoodGroceriesSupport {TotalAmount = 100, SupportDelivery = new Interac { NotificationEmail = "test@test.com", ReceivingRegistrantId = registrant.Id } },
                new FoodRestaurantSupport { TotalAmount = 100, SupportDelivery = new Referral { SupplierDetails = new SupplierDetails { Id = TestData.SupplierCId } } },
                new LodgingBilletingSupport() { NumberOfNights = 1, SupportDelivery = new Referral { IssuedToPersonName = "test person" } },
                new LodgingGroupSupport { NumberOfNights = 1, FacilityCommunityCode = TestData.RandomCommunity, SupportDelivery = new Referral { IssuedToPersonName = "test person" } },
                new LodgingHotelSupport { NumberOfNights = 1, NumberOfRooms = 1, SupportDelivery = new Referral { IssuedToPersonName = "test person" } },
                new TransportationOtherSupport { TotalAmount = 100, SupportDelivery = new Interac { NotificationEmail = "test@test.com", ReceivingRegistrantId = registrant.Id } },
                new TransportationTaxiSupport { SupportDelivery = new Referral { IssuedToPersonName = "test person" } },
            };

            foreach (var s in supports)
            {
                s.From = DateTime.UtcNow;
                s.To = DateTime.UtcNow.AddDays(3);
            }

            var printRequestId = await manager.Handle(new ProcessSupportsCommand { FileId = fileId, Supports = supports, RequestingUserId = TestData.Tier4TeamMemberId });

            printRequestId.ShouldNotBeNullOrEmpty();

            var refreshedFile = (await manager.Handle(new EvacuationFilesQuery { FileId = fileId })).Items.ShouldHaveSingleItem();
            refreshedFile.Supports.ShouldNotBeEmpty();
            refreshedFile.Supports.Count().ShouldBe(supports.Length);
            foreach (var support in refreshedFile.Supports)
            {
                var sourceSupport = supports.Where(s => s.GetType() == support.GetType()).ShouldHaveSingleItem();
                if (support.SupportDelivery is Referral r && sourceSupport.SupportDelivery is Referral sourceReferral && r.SupplierDetails != null)
                {
                    r.SupplierDetails.ShouldNotBeNull();
                    r.SupplierDetails.Id.ShouldBe(sourceReferral.SupplierDetails.Id);
                    r.SupplierDetails.Name.ShouldNotBeNull();
                    r.SupplierDetails.Address.ShouldNotBeNull();
                }
                support.CreatedBy.Id.ShouldBe(TestData.Tier4TeamMemberId);
                support.CreatedOn.ShouldNotBeNull().ShouldBeInRange(DateTime.UtcNow.AddSeconds(-30), DateTime.UtcNow);
                support.IssuedOn.ShouldNotBeNull().ShouldBeInRange(DateTime.UtcNow.AddSeconds(-30), DateTime.UtcNow);
                support.IssuedBy.ShouldNotBeNull().Id.ShouldBe(TestData.Tier4TeamMemberId);
            }
        }

        [Fact(Skip = RequiresVpnConnectivity)]
        public async Task CanVoidReferral()
        {
            var registrant = await GetRegistrantByUserId(TestData.ContactUserId);
            var file = CreateNewTestEvacuationFile(registrant);

            file.NeedsAssessment.CompletedOn = DateTime.UtcNow;
            file.NeedsAssessment.CompletedBy = new TeamMember { Id = TestData.Tier4TeamMemberId };

            var fileId = await manager.Handle(new SubmitEvacuationFileCommand { File = file });

            var supports = new Support[]
            {
                new ClothingSupport { SupportDelivery = new Referral { SupplierDetails = new SupplierDetails { Id = TestData.SupplierAId } } },
                new IncidentalsSupport { SupportDelivery = new Interac { NotificationEmail = "test@test.com", ReceivingRegistrantId = registrant.Id } },
                new FoodGroceriesSupport {  SupportDelivery = new Referral { SupplierDetails = new SupplierDetails { Id = TestData.SupplierBId } } },
                new FoodRestaurantSupport {  SupportDelivery = new Referral { SupplierDetails = new SupplierDetails { Id = TestData.SupplierCId } } },
                new LodgingBilletingSupport() { NumberOfNights = 1, SupportDelivery = new Referral { IssuedToPersonName = "test person" } },
                new LodgingGroupSupport { NumberOfNights = 1, FacilityCommunityCode = TestData.RandomCommunity, SupportDelivery = new Referral { IssuedToPersonName = "test person" } },
                new LodgingHotelSupport { NumberOfNights = 1, NumberOfRooms = 1, SupportDelivery = new Referral { IssuedToPersonName = "test person" } },
                new TransportationOtherSupport { SupportDelivery = new Referral { IssuedToPersonName = "test person" }},
                new TransportationTaxiSupport { SupportDelivery = new Referral { IssuedToPersonName = "test person" }},
            };

            foreach (var s in supports)
            {
                s.From = DateTime.UtcNow;
                s.To = DateTime.UtcNow.AddDays(3);
            }

            await manager.Handle(new ProcessSupportsCommand { FileId = fileId, Supports = supports, RequestingUserId = TestData.Tier4TeamMemberId });

            var support = (await manager.Handle(new SearchSupportsQuery { FileId = fileId })).Items.First(s => s.SupportDelivery is Referral);

            await manager.Handle(new VoidSupportCommand
            {
                FileId = fileId,
                SupportId = support.Id,
                VoidReason = SupportVoidReason.ErrorOnPrintedReferral
            });

            var updatedSupport = (await manager.Handle(new SearchSupportsQuery { FileId = fileId })).Items.Single(s => s.Id == support.Id);

            updatedSupport.Status.ShouldBe(SupportStatus.Void);
        }

        [Fact(Skip = RequiresVpnConnectivity)]
        public async Task CanCancelETransfer()
        {
            var registrant = await GetRegistrantByUserId(TestData.ContactUserId);
            var file = CreateNewTestEvacuationFile(registrant);

            file.NeedsAssessment.CompletedOn = DateTime.UtcNow;
            file.NeedsAssessment.CompletedBy = new TeamMember { Id = TestData.Tier4TeamMemberId };

            var fileId = await manager.Handle(new SubmitEvacuationFileCommand { File = file });

            var supports = new Support[]
            {
                new ClothingSupport { SupportDelivery = new Referral { SupplierDetails = new SupplierDetails { Id = TestData.SupplierAId } } },
                new IncidentalsSupport { SupportDelivery = new Interac { NotificationEmail = "test@test.com", ReceivingRegistrantId = registrant.Id } },
                new FoodGroceriesSupport {  SupportDelivery = new Referral { SupplierDetails = new SupplierDetails { Id = TestData.SupplierBId } } },
                new FoodRestaurantSupport {  SupportDelivery = new Referral { SupplierDetails = new SupplierDetails { Id = TestData.SupplierCId } } },
                new LodgingBilletingSupport() { NumberOfNights = 1, SupportDelivery = new Referral { IssuedToPersonName = "test person" } },
                new LodgingGroupSupport { NumberOfNights = 1, FacilityCommunityCode = TestData.RandomCommunity, SupportDelivery = new Referral { IssuedToPersonName = "test person" } },
                new LodgingHotelSupport { NumberOfNights = 1, NumberOfRooms = 1, SupportDelivery = new Referral { IssuedToPersonName = "test person" } },
                new TransportationOtherSupport { SupportDelivery = new Referral { IssuedToPersonName = "test person" }},
                new TransportationTaxiSupport { SupportDelivery = new Referral { IssuedToPersonName = "test person" }},
            };

            foreach (var s in supports)
            {
                s.From = DateTime.UtcNow;
                s.To = DateTime.UtcNow.AddDays(3);
            }

            await manager.Handle(new ProcessSupportsCommand { FileId = fileId, Supports = supports, RequestingUserId = TestData.Tier4TeamMemberId });

            var support = (await manager.Handle(new SearchSupportsQuery { FileId = fileId })).Items.First(s => s.SupportDelivery is ETransfer);

            await manager.Handle(new CancelSupportCommand
            {
                FileId = fileId,
                SupportId = support.Id,
                Reason = "need to cancel"
            });

            var updatedSupport = (await manager.Handle(new SearchSupportsQuery { FileId = fileId })).Items.Single(s => s.Id == support.Id);

            updatedSupport.Status.ShouldBe(SupportStatus.Cancelled);
        }

        [Fact(Skip = RequiresVpnConnectivity)]
        public async Task CanReprintSupport()
        {
            var printRequestId = await manager.Handle(new ReprintSupportCommand
            {
                FileId = TestData.EvacuationFileId,
                ReprintReason = "test",
                RequestingUserId = TestData.Tier4TeamMemberId,
                SupportId = TestData.SupportIds.First()
            });

            printRequestId.ShouldNotBeNullOrEmpty();
        }

        [Fact(Skip = RequiresVpnConnectivity)]
        public async Task CanQuerySupplierList()
        {
            var taskId = TestData.ActiveTaskId;
            var list = (await manager.Handle(new SuppliersListQuery { TaskId = taskId })).Items;
            list.ShouldNotBeEmpty();
        }

        [Fact(Skip = RequiresVpnConnectivity)]
        public async Task CanQueryPrintRequest()
        {
            var dynamicsContext = Services.GetRequiredService<EssContext>();
            var testPrintRequest = dynamicsContext.era_referralprints
                .Where(pr => pr.statecode == (int)EntityState.Active && pr._era_requestinguserid_value != null)
                .OrderByDescending(pr => pr.createdon)
                .Take(new Random().Next(1, 20))
                .ToArray()
                .First();

            var response = await manager.Handle(new PrintRequestQuery
            {
                PrintRequestId = testPrintRequest.era_referralprintid.ToString(),
                RequestingUserId = testPrintRequest._era_requestinguserid_value?.ToString()
            });
            await File.WriteAllBytesAsync("./newTestPrintRequestFile.pdf", response.Content);
        }

        [Fact(Skip = RequiresVpnConnectivity)]
        public async Task ProcessSupportsCommand_DigitalAndPaperSupports_BusinessValidationException()
        {
            var fileId = TestData.PaperEvacuationFileId;

            var supports = new Support[]
            {
                new IncidentalsSupport() { SupportDelivery = new Referral { ManualReferralId = $"{TestData.TestPrefix}-paperreferral"} },
                new IncidentalsSupport() { SupportDelivery = new Referral() }
            };

            await Should.ThrowAsync<BusinessValidationException>(async () => await manager.Handle(new ProcessSupportsCommand
            {
                FileId = fileId,
                Supports = supports,
                RequestingUserId = TestData.Tier4TeamMemberId
            }));
        }

        [Fact(Skip = RequiresVpnConnectivity)]
        public async Task CanProcessPaperReferrals()
        {
            var registrant = await GetRegistrantByUserId(TestData.ContactUserId);
            var paperFile = CreateNewTestEvacuationFile(registrant);

            paperFile.ExternalReferenceId = $"{TestData.TestPrefix}-paperfile";
            paperFile.NeedsAssessment.CompletedOn = DateTime.UtcNow;
            paperFile.NeedsAssessment.CompletedBy = new TeamMember { Id = TestData.Tier4TeamMemberId };

            var fileId = await manager.Handle(new SubmitEvacuationFileCommand { File = paperFile });

            var supports = new Support[]
            {
                new ClothingSupport { SupportDelivery = new Referral { SupplierDetails = new SupplierDetails { Id = TestData.SupplierAId } } },
                new IncidentalsSupport { SupportDelivery = new Referral { SupplierDetails = new SupplierDetails { Id = TestData.SupplierBId } } },
                new FoodGroceriesSupport {  SupportDelivery = new Referral { SupplierDetails = new SupplierDetails { Id = TestData.SupplierBId } } },
                new FoodRestaurantSupport {  SupportDelivery = new Referral { SupplierDetails = new SupplierDetails { Id = TestData.SupplierCId } } },
                new LodgingBilletingSupport() { NumberOfNights = 1, SupportDelivery = new Referral { IssuedToPersonName = "test person" } },
                new LodgingGroupSupport { NumberOfNights = 1, FacilityCommunityCode = TestData.RandomCommunity, SupportDelivery = new Referral { IssuedToPersonName = "test person" } },
                new LodgingHotelSupport { NumberOfNights = 1, NumberOfRooms = 1, SupportDelivery = new Referral { IssuedToPersonName = "test person" } },
                new TransportationOtherSupport { SupportDelivery = new Referral { IssuedToPersonName = "test person" }},
                new TransportationTaxiSupport { SupportDelivery = new Referral { IssuedToPersonName = "test person" }},
            };

            foreach (var s in supports)
            {
                s.From = DateTime.UtcNow;
                s.To = DateTime.UtcNow.AddDays(3);
                s.IssuedOn = DateTime.Parse("2021/12/31T16:14:32Z");
                ((Referral)s.SupportDelivery).ManualReferralId = $"{TestData.TestPrefix}-paperreferral";
                s.IssuedBy = new TeamMember { DisplayName = "autotest R" };
            }

            await manager.Handle(new ProcessPaperSupportsCommand { FileId = fileId, Supports = supports, RequestingUserId = TestData.Tier4TeamMemberId });

            var refreshedFile = (await manager.Handle(new EvacuationFilesQuery { FileId = fileId })).Items.ShouldHaveSingleItem();
            refreshedFile.Supports.ShouldNotBeEmpty();
            refreshedFile.Supports.Count().ShouldBe(supports.Length);
            foreach (var support in refreshedFile.Supports)
            {
                var sourceSupport = supports.Where(s => s.GetType() == support.GetType()).ShouldHaveSingleItem();
                if (support.SupportDelivery is Referral r && sourceSupport.SupportDelivery is Referral sourceReferral && r.SupplierDetails != null)
                {
                    r.SupplierDetails.ShouldNotBeNull();
                    r.SupplierDetails.Id.ShouldBe(sourceReferral.SupplierDetails.Id);
                    r.SupplierDetails.Name.ShouldNotBeNull();
                    r.SupplierDetails.Address.ShouldNotBeNull();
                    r.ManualReferralId.ShouldBe(sourceReferral.ManualReferralId);
                }
                support.CreatedBy.Id.ShouldBe(TestData.Tier4TeamMemberId);
                support.CreatedOn.ShouldNotBeNull().ShouldBeInRange(DateTime.UtcNow.AddSeconds(-30), DateTime.UtcNow);
                support.IssuedBy.ShouldNotBeNull().DisplayName.ShouldBe(sourceSupport.IssuedBy.DisplayName);
                support.IssuedOn.ShouldNotBeNull().ShouldBe(sourceSupport.IssuedOn.ShouldNotBeNull());
            }
        }

        [Fact(Skip = RequiresVpnConnectivity)]
        public async Task ProcessPaperSupportsCommand_DuplicateReferralIdAndType_BusinessValidationException()
        {
            var fileId = TestData.PaperEvacuationFileId;

            var supports = new Support[]
            {
                new IncidentalsSupport() { SupportDelivery = new Referral { ManualReferralId = $"{TestData.TestPrefix}-paperreferral" } },
                new IncidentalsSupport() {  SupportDelivery = new Referral { ManualReferralId = $"{TestData.TestPrefix}-paperreferral" } }
            };

            await Should.ThrowAsync<BusinessValidationException>(async () => await manager.Handle(new ProcessPaperSupportsCommand
            {
                FileId = fileId,
                Supports = supports,
                RequestingUserId = TestData.Tier4TeamMemberId
            }));
        }

        [Fact(Skip = RequiresVpnConnectivity)]
        public async Task ProcessPaperSupportsCommand_DigitalAndPaperSupports_BusinessValidationException()
        {
            var fileId = TestData.PaperEvacuationFileId;

            var supports = new Support[]
            {
                new IncidentalsSupport() { SupportDelivery = new Referral { ManualReferralId = $"{TestData.TestPrefix}-paperreferral" } },
                new IncidentalsSupport(){ SupportDelivery = new Referral() }
            };

            await Should.ThrowAsync<BusinessValidationException>(async () => await manager.Handle(new ProcessPaperSupportsCommand
            {
                FileId = fileId,
                Supports = supports,
                RequestingUserId = TestData.Tier4TeamMemberId
            }));
        }

        [Fact(Skip = RequiresVpnConnectivity)]
        public async Task SearchSupports_ExternalReferenceId_CorrectListOfSupports()
        {
            var fileId = TestData.PaperEvacuationFileId;

            var newSupports = new Support[]
            {
                new ClothingSupport { SupportDelivery = new Referral { SupplierDetails = new SupplierDetails { Id = TestData.SupplierAId } } },
                new IncidentalsSupport { SupportDelivery = new Referral { SupplierDetails = new SupplierDetails { Id = TestData.SupplierBId } } },
                new FoodGroceriesSupport {  SupportDelivery = new Referral { SupplierDetails = new SupplierDetails { Id = TestData.SupplierBId } } },
                new FoodRestaurantSupport {  SupportDelivery = new Referral { SupplierDetails = new SupplierDetails { Id = TestData.SupplierCId } } },
                new LodgingBilletingSupport() { NumberOfNights = 1, SupportDelivery = new Referral { IssuedToPersonName = "test person" } },
                new LodgingGroupSupport { NumberOfNights = 1, FacilityCommunityCode = TestData.RandomCommunity, SupportDelivery = new Referral { IssuedToPersonName = "test person" } },
                new LodgingHotelSupport { NumberOfNights = 1, NumberOfRooms = 1, SupportDelivery = new Referral { IssuedToPersonName = "test person" } },
                new TransportationOtherSupport { SupportDelivery = new Referral { IssuedToPersonName = "test person" }},
                new TransportationTaxiSupport { SupportDelivery = new Referral { IssuedToPersonName = "test person" }},
            };

            var uniqueId = Guid.NewGuid().ToString().Substring(0, 4);
            var paperReferralId = $"{TestData.TestPrefix}-paperreferral-{uniqueId}";

            foreach (var s in newSupports)
            {
                s.From = DateTime.UtcNow;
                s.To = DateTime.UtcNow.AddDays(3);
                s.IssuedOn = DateTime.Parse("2021/12/31T16:14:32Z");
                ((Referral)s.SupportDelivery).ManualReferralId = paperReferralId;
                s.IssuedBy = new TeamMember { DisplayName = "autotest R" };
            }

            await manager.Handle(new ProcessPaperSupportsCommand { FileId = fileId, RequestingUserId = TestData.Tier4TeamMemberId, Supports = newSupports });

            var supports = (await manager.Handle(new SearchSupportsQuery { ExternalReferenceId = paperReferralId })).Items;
            supports.ShouldNotBeEmpty();
            supports.Count().ShouldBe(newSupports.Length);
            foreach (var support in supports)
            {
                var referral = support.SupportDelivery.ShouldBeAssignableTo<Referral>().ShouldNotBeNull();
                support.FileId.ShouldBe(fileId);
                support.OriginatingNeedsAssessmentId.ShouldBe(TestData.PaperEvacuationFileNeedsAssessmentId);
                referral.ManualReferralId.ShouldBe(paperReferralId);
            }
        }
    }
}
