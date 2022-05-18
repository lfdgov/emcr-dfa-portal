﻿using OpenQA.Selenium;

namespace EMBC.Tests.Automation.Responders.PageObjects
{
    public class WizardEssFile : WizardPageObjectBase
    {
        //ELEMENTS:
        private By evacDetailsFormEvacAddressYesRadioBttn = By.Id("addressYesOption");
        private By evacDetailsFacilityNameInput = By.CssSelector("input[formcontrolname='facilityName']");
        private By evacDetailsInsuranceRadioBttn = By.CssSelector("mat-radio-group[formcontrolname='insurance'] mat-radio-button[id='Yes']");
        private By evacDetailsDescriptionTextarea = By.CssSelector("textarea[formcontrolname='householdAffected']");
        
        private By evacDetailsCompletedDateInput = By.CssSelector("input[formcontrolname='paperCompletedOn']");
        private By evacDetailsCompletedTimeInput = By.CssSelector("input[formcontrolname='paperCompletedTime']");

        private By householdFormAllMembersCheckbox = By.XPath("//app-household-members[1]/div[1]/div[2]/div[1]/div[1]/div[1]/form[1]/div[3]/div[1]/mat-table[1]/mat-header-row[1]/th[1]/mat-checkbox[1]");
        private By householdFormHasMembersYesRadioBttn = By.XPath("//mat-radio-group[@formcontrolname='hasHouseholdMembers']/mat-radio-button[1]");
        private By householdFormHasMembersNoRadioBttn = By.XPath("//mat-radio-group[@formcontrolname='hasHouseholdMembers']/mat-radio-button[2]");
        private By householfFormHasDietaryReqsYesRadioBttn = By.XPath("//mat-radio-group[@formcontrolname='hasSpecialDiet']/mat-radio-button[1]");
        private By householfFormHasDietaryReqsNoRadioBttn = By.XPath("//mat-radio-group[@formcontrolname='hasSpecialDiet']/mat-radio-button[2]");
        private By householdFormDietDetailsTextarea = By.CssSelector("textarea[formcontrolname='specialDietDetails']");
        private By householdFormHasMedicineReqYesRadioBttn = By.XPath("//mat-radio-group[@formcontrolname='hasMedication']/mat-radio-button[1]");
        private By householdFormHasMedicineReqNoRadioBttn = By.XPath("//mat-radio-group[@formcontrolname='hasMedication']/mat-radio-button[2]");
        private By householdFormMedicineSupplyYesRadioBttn = By.XPath("//mat-radio-group[@formcontrolname='medicationSupply']/mat-radio-button[1]");

        private By animalFormHasPetsYesRadioBttn = By.XPath("//mat-radio-group[@formcontrolname='hasPets']/mat-radio-button[1]");
        private By animalFormHasPetsNoRadioBttn = By.XPath("//mat-radio-group[@formcontrolname='hasPets']/mat-radio-button[2]");
        private By animalFormTypeInput = By.CssSelector("input[formcontrolname='type']");
        private By animalFormQuantityInput = By.CssSelector("input[formcontrolname='quantity']");
        private By animalFormHasPetFoodYesRadioBttn = By.CssSelector("mat-radio-group[formcontrolname='hasPetsFood'] mat-radio-button:nth-child(1)");

        private By needsFormFoodYesRadioBttn = By.XPath("//mat-radio-group[@formcontrolname='canEvacueeProvideFood']/mat-radio-button[1]");
        private By needsFormLodgeYesRadioBttn = By.XPath("//mat-radio-group[@formcontrolname='canEvacueeProvideLodging']/mat-radio-button[1]");
        private By needsFormClothesYesRadioBttn = By.XPath("//mat-radio-group[@formcontrolname='canEvacueeProvideClothing']/mat-radio-button[1]");
        private By needsFormTransportYesRadioBttn = By.XPath("//mat-radio-group[@formcontrolname='canEvacueeProvideTransportation']/mat-radio-button[1]");
        private By needsFormIncidentalYesRadioBttn = By.XPath("//mat-radio-group[@formcontrolname='canEvacueeProvideIncidentals']/mat-radio-button[1]");

        private By securityPhraseFormPhraseInput = By.CssSelector("input[formcontrolname='securityPhrase']");

        private By bypassEssFileReviewTab = By.XPath("//app-step-ess-file[1]/div[1]/nav[1]/div[2]/div[1]/div[1]/a[6]/span[1]");

        //FUNCTIONS
        public WizardEssFile(IWebDriver webDriver) : base(webDriver)
        { }

        public void WizardOnlineEvacDetailsFormReqFields(string facilityLocation, string householdAffected)
        {
            Wait();

            RadioButtonElement(evacDetailsFormEvacAddressYesRadioBttn);
            webDriver.FindElement(evacDetailsFacilityNameInput).SendKeys(facilityLocation);
            RadioButtonElement(evacDetailsInsuranceRadioBttn);
            webDriver.FindElement(evacDetailsDescriptionTextarea).SendKeys(householdAffected);
            ButtonElement("Next");
        }

        public void WizardPaperBasedEvacDetailsFormReqFields(string interviewer, string initials, string completedDate, string completedTime, string addressLine1, string city, string facilityLocation, string householdAffected)
        {
            Wait();

            webDriver.FindElement(personFormFirstNameInput).SendKeys(interviewer);
            webDriver.FindElement(wizardInterviewerInitialsInput).SendKeys(initials);
            webDriver.FindElement(evacDetailsCompletedDateInput).SendKeys(completedDate);
            webDriver.FindElement(evacDetailsCompletedTimeInput).SendKeys(completedTime);
            webDriver.FindElement(addressFormAddressLine1Input).SendKeys(addressLine1);
            webDriver.FindElement(addressFormCityInput).SendKeys(city);
           
            Wait();

            var citySelect = webDriver.FindElement(By.Id("Victoria"));
            citySelect.Click();

            webDriver.FindElement(evacDetailsFacilityNameInput).SendKeys(facilityLocation);
            RadioButtonElement(evacDetailsInsuranceRadioBttn);
            webDriver.FindElement(evacDetailsDescriptionTextarea).SendKeys(householdAffected);
            ButtonElement("Next");
        }

        public void WizardHouseholdMembersMinForm()
        {
            Wait();

            RadioButtonElement(householdFormHasMembersNoRadioBttn);
            RadioButtonElement(householfFormHasDietaryReqsNoRadioBttn);
            RadioButtonElement(householdFormHasMedicineReqNoRadioBttn);
            ButtonElement("Next");
        }

        public void WizardHouseholdMembersMaxForm(string firstName, string lastName, string gender, string dob, string dietDetails)
        {
            Wait();

            RadioButtonElement(householdFormHasMembersYesRadioBttn);

            Wait();

            webDriver.FindElement(personFormFirstNameInput).SendKeys(firstName);
            webDriver.FindElement(personFormLastNameInput).SendKeys(lastName);
            webDriver.FindElement(personFormGenderSelect).SendKeys(gender);
            webDriver.FindElement(personFormDateOfBirthInput).SendKeys(dob);
            ButtonElement("Save");

            RadioButtonElement(householfFormHasDietaryReqsYesRadioBttn);
            webDriver.FindElement(householdFormDietDetailsTextarea).SendKeys(dietDetails);
            RadioButtonElement(householdFormHasMedicineReqYesRadioBttn);
            RadioButtonElement(householdFormMedicineSupplyYesRadioBttn);
            ButtonElement("Next");
        }

        public void AnimalsMinForm()
        {
            RadioButtonElement(animalFormHasPetsNoRadioBttn);
            ButtonElement("Next");
        }

        public void AnimalsMaxForm(string petType, string petQuantity)
        {
            RadioButtonElement(animalFormHasPetsYesRadioBttn);
            webDriver.FindElement(animalFormTypeInput).SendKeys(petType);
            webDriver.FindElement(animalFormQuantityInput).SendKeys(petQuantity);
            ButtonElement("Save");

            RadioButtonElement(animalFormHasPetFoodYesRadioBttn);
            ButtonElement("Next");
        }

        public void WizardNeedsAssessmentsForm()
        {
            Wait();

            RadioButtonElement(needsFormFoodYesRadioBttn);
            RadioButtonElement(needsFormLodgeYesRadioBttn);
            RadioButtonElement(needsFormClothesYesRadioBttn);
            RadioButtonElement(needsFormTransportYesRadioBttn);
            RadioButtonElement(needsFormIncidentalYesRadioBttn);
 
            ButtonElement("Next");
        }

        public void WizardSecurityPhraseForm(string securityPhrase)
        {
           Wait();

           webDriver.FindElement(securityPhraseFormPhraseInput).SendKeys(securityPhrase);
           ButtonElement("Next");
        }

        public void WizardEditESSFilePassStep()
        {
            ButtonElement("Next");

            webDriver.FindElement(householdFormAllMembersCheckbox).Click();
            webDriver.FindElement(bypassEssFileReviewTab).Click();

            ButtonElement("Save ESS File");
        }

        public void WizardReviewEssFile()
        {
            this.ButtonElement("Save");
        }
   
    }
}