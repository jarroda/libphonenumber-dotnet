/*
 * Copyright (C) 2009 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Xml.Linq;
using Xunit;

namespace PhoneNumbers.Test
{
    public class TestBuildMetadataFromXml
    {
        // Helper method that outputs a DOM element from a XML string.
        private static XElement parseXmlString(String xmlString)
        {

            var document = XElement.Parse(xmlString);
            return document;
        }

        // Tests validateRE().
        [Fact]
        public void TestValidateRERemovesWhiteSpaces()
        {
            String input = " hello world ";
            // Should remove all the white spaces contained in the provided string.
            Assert.Equal("helloworld", BuildMetadataFromXml.ValidateRE(input, true));
            // Make sure it only happens when the last parameter is set to true.
            Assert.Equal(" hello world ", BuildMetadataFromXml.ValidateRE(input, false));
        }

        [Fact]
        public void TestValidateREThrowsException()
        {
            String invalidPattern = "[";
            // Should throw an exception when an invalid pattern is provided independently of the last
            // parameter (remove white spaces).
            Assert.ThrowsAny<ArgumentException>(() =>
            {
                BuildMetadataFromXml.ValidateRE(invalidPattern, false);
            });

            Assert.ThrowsAny<ArgumentException>(() =>
            {
                BuildMetadataFromXml.ValidateRE(invalidPattern, true);
            });
        }

        [Fact]
        public void TestValidateRE()
        {
            String validPattern = "[a-zA-Z]d{1,9}";
            // The provided pattern should be left unchanged.
            Assert.Equal(validPattern, BuildMetadataFromXml.ValidateRE(validPattern, false));
        }

        // Tests NationalPrefix.
        [Fact]
        public void TestGetNationalPrefix()
        {
            String xmlInput = "<territory nationalPrefix='00'/>";
            var territoryElement = parseXmlString(xmlInput);
            Assert.Equal("00", BuildMetadataFromXml.GetNationalPrefix(territoryElement));
        }

        // Tests LoadTerritoryTagMetadata().
        [Fact]
        public void TestLoadTerritoryTagMetadata()
        {
            String xmlInput =
                "<territory countryCode='33' leadingDigits='2' internationalPrefix='00'" +
                "           preferredInternationalPrefix='0011' nationalPrefixForParsing='0'" +
                "           nationalPrefixTransformRule='9$1'" + // nationalPrefix manually injected.
                "           preferredExtnPrefix=' x' mainCountryForCode='true'" +
                "           leadingZeroPossible='true'>" +
                "</territory>";
            XElement territoryElement = parseXmlString(xmlInput);
            PhoneMetadata.Builder phoneMetadata =
                BuildMetadataFromXml.LoadTerritoryTagMetadata("33", territoryElement, "0");
            Assert.Equal(33, phoneMetadata.CountryCode);
            Assert.Equal("2", phoneMetadata.LeadingDigits);
            Assert.Equal("00", phoneMetadata.InternationalPrefix);
            Assert.Equal("0011", phoneMetadata.PreferredInternationalPrefix);
            Assert.Equal("0", phoneMetadata.NationalPrefixForParsing);
            Assert.Equal("9$1", phoneMetadata.NationalPrefixTransformRule);
            Assert.Equal("0", phoneMetadata.NationalPrefix);
            Assert.Equal(" x", phoneMetadata.PreferredExtnPrefix);
            Assert.True(phoneMetadata.MainCountryForCode);
            Assert.True(phoneMetadata.LeadingZeroPossible);
        }

        [Fact]
        public void TestLoadTerritoryTagMetadataSetsBooleanFieldsToFalseByDefault()
        {
            String xmlInput = "<territory countryCode='33'/>";
            XElement territoryElement = parseXmlString(xmlInput);
            PhoneMetadata.Builder phoneMetadata =
                BuildMetadataFromXml.LoadTerritoryTagMetadata("33", territoryElement, "");
            Assert.False(phoneMetadata.MainCountryForCode);
            Assert.False(phoneMetadata.LeadingZeroPossible);
        }

        [Fact]
        public void TestLoadTerritoryTagMetadataSetsNationalPrefixForParsingByDefault()
        {
            String xmlInput = "<territory countryCode='33'/>";
            XElement territoryElement = parseXmlString(xmlInput);
            PhoneMetadata.Builder phoneMetadata =
                BuildMetadataFromXml.LoadTerritoryTagMetadata("33", territoryElement, "00");
            // When unspecified, nationalPrefixForParsing defaults to nationalPrefix.
            Assert.Equal("00", phoneMetadata.NationalPrefix);
            Assert.Equal(phoneMetadata.NationalPrefix, phoneMetadata.NationalPrefixForParsing);
        }

        [Fact]
        public void TestLoadTerritoryTagMetadataWithRequiredAttributesOnly()
        {
            String xmlInput = "<territory countryCode='33' internationalPrefix='00'/>";
            XElement territoryElement = parseXmlString(xmlInput);
            // Should not throw any exception.
            BuildMetadataFromXml.LoadTerritoryTagMetadata("33", territoryElement, "");
        }

        // Tests loadInternationalFormat().
        [Fact]
        public void TestLoadInternationalFormat()
        {
            String intlFormat = "$1 $2";
            String xmlInput = "<numberFormat><intlFormat>" + intlFormat + "</intlFormat></numberFormat>";
            XElement numberFormatElement = parseXmlString(xmlInput);
            PhoneMetadata.Builder metadata = new PhoneMetadata.Builder();
            String nationalFormat = "";

            Assert.True(BuildMetadataFromXml.LoadInternationalFormat(metadata, numberFormatElement,
                                                                    nationalFormat));
            Assert.Equal(intlFormat, metadata.IntlNumberFormatList[0].Format);
        }

        [Fact]
        public void TestLoadInternationalFormatWithBothNationalAndIntlFormatsDefined()
        {
            String intlFormat = "$1 $2";
            String xmlInput = "<numberFormat><intlFormat>" + intlFormat + "</intlFormat></numberFormat>";
            XElement numberFormatElement = parseXmlString(xmlInput);
            PhoneMetadata.Builder metadata = new PhoneMetadata.Builder();
            String nationalFormat = "$1";

            Assert.True(BuildMetadataFromXml.LoadInternationalFormat(metadata, numberFormatElement,
                                                                    nationalFormat));
            Assert.Equal(intlFormat, metadata.IntlNumberFormatList[0].Format);
        }

        [Fact]
        public void TestLoadInternationalFormatExpectsOnlyOnePattern()
        {
            String xmlInput = "<numberFormat><intlFormat/><intlFormat/></numberFormat>";
            XElement numberFormatElement = parseXmlString(xmlInput);
            PhoneMetadata.Builder metadata = new PhoneMetadata.Builder();

            // Should throw an exception as multiple intlFormats are provided.
            Assert.ThrowsAny<Exception>(() =>
            {
                BuildMetadataFromXml.LoadInternationalFormat(metadata, numberFormatElement, "");
            });
        }

        [Fact]
        public void TestLoadInternationalFormatUsesNationalFormatByDefault()
        {
            String xmlInput = "<numberFormat></numberFormat>";
            XElement numberFormatElement = parseXmlString(xmlInput);
            PhoneMetadata.Builder metadata = new PhoneMetadata.Builder();
            String nationalFormat = "$1 $2 $3";

            Assert.False(BuildMetadataFromXml.LoadInternationalFormat(metadata, numberFormatElement,
                                                                     nationalFormat));
            Assert.Equal(nationalFormat, metadata.IntlNumberFormatList[0].Format);
        }

        // Tests LoadNationalFormat().
        [Fact]
        public void TestLoadNationalFormat()
        {
            String nationalFormat = "$1 $2";
            String xmlInput = String.Format("<numberFormat><format>{0}</format></numberFormat>",
                                            nationalFormat);
            XElement numberFormatElement = parseXmlString(xmlInput);
            PhoneMetadata.Builder metadata = new PhoneMetadata.Builder();
            NumberFormat.Builder numberFormat = new NumberFormat.Builder();

            Assert.Equal(nationalFormat,
                         BuildMetadataFromXml.LoadNationalFormat(metadata, numberFormatElement,
                                                                 numberFormat));
        }

        [Fact]
        public void TestLoadNationalFormatRequiresFormat()
        {
            String xmlInput = "<numberFormat></numberFormat>";
            XElement numberFormatElement = parseXmlString(xmlInput);
            PhoneMetadata.Builder metadata = new PhoneMetadata.Builder();
            NumberFormat.Builder numberFormat = new NumberFormat.Builder();

            Assert.ThrowsAny<Exception>(() =>
            {
                BuildMetadataFromXml.LoadNationalFormat(metadata, numberFormatElement, numberFormat);
            });
        }

        [Fact]
        public void TestLoadNationalFormatExpectsExactlyOneFormat()
        {
            String xmlInput = "<numberFormat><format/><format/></numberFormat>";
            XElement numberFormatElement = parseXmlString(xmlInput);
            PhoneMetadata.Builder metadata = new PhoneMetadata.Builder();
            NumberFormat.Builder numberFormat = new NumberFormat.Builder();

            Assert.ThrowsAny<Exception>(() =>
            {
                BuildMetadataFromXml.LoadNationalFormat(metadata, numberFormatElement, numberFormat);
            });
        }

        // Tests loadAvailableFormats().
        [Fact]
        public void TestLoadAvailableFormats()
        {
            String xmlInput =
                "<territory >" +
                "  <availableFormats>" +
                "    <numberFormat nationalPrefixFormattingRule='($FG)'" +
                "                  carrierCodeFormattingRule='$NP $CC ($FG)'>" +
                "      <format>$1 $2 $3</format>" +
                "    </numberFormat>" +
                "  </availableFormats>" +
                "</territory>";
            XElement element = parseXmlString(xmlInput);
            PhoneMetadata.Builder metadata = new PhoneMetadata.Builder();
            BuildMetadataFromXml.LoadAvailableFormats(
                metadata, element, "0", "", false /* NP not optional */);
            Assert.Equal("(${1})", metadata.NumberFormatList[0].NationalPrefixFormattingRule);
            Assert.Equal("0 $CC (${1})", metadata.NumberFormatList[0].DomesticCarrierCodeFormattingRule);
            Assert.Equal("$1 $2 $3", metadata.NumberFormatList[0].Format);
        }

        [Fact]
        public void TestLoadAvailableFormatsPropagatesCarrierCodeFormattingRule()
        {
            String xmlInput =
                "<territory carrierCodeFormattingRule='$NP $CC ($FG)'>" +
                "  <availableFormats>" +
                "    <numberFormat nationalPrefixFormattingRule='($FG)'>" +
                "      <format>$1 $2 $3</format>" +
                "    </numberFormat>" +
                "  </availableFormats>" +
                "</territory>";
            XElement element = parseXmlString(xmlInput);
            PhoneMetadata.Builder metadata = new PhoneMetadata.Builder();
            BuildMetadataFromXml.LoadAvailableFormats(
                metadata, element, "0", "", false /* NP not optional */);
            Assert.Equal("(${1})", metadata.NumberFormatList[0].NationalPrefixFormattingRule);
            Assert.Equal("0 $CC (${1})", metadata.NumberFormatList[0].DomesticCarrierCodeFormattingRule);
            Assert.Equal("$1 $2 $3", metadata.NumberFormatList[0].Format);
        }

        [Fact]
        public void TestLoadAvailableFormatsSetsProvidedNationalPrefixFormattingRule()
        {
            String xmlInput =
                "<territory>" +
                "  <availableFormats>" +
                "    <numberFormat><format>$1 $2 $3</format></numberFormat>" +
                "  </availableFormats>" +
                "</territory>";
            XElement element = parseXmlString(xmlInput);
            PhoneMetadata.Builder metadata = new PhoneMetadata.Builder();
            BuildMetadataFromXml.LoadAvailableFormats(
                metadata, element, "0", "($1)", false /* NP not optional */);
            Assert.Equal("($1)", metadata.NumberFormatList[0].NationalPrefixFormattingRule);
        }

        [Fact]
        public void TestLoadAvailableFormatsClearsIntlFormat()
        {
            String xmlInput =
                "<territory>" +
                "  <availableFormats>" +
                "    <numberFormat><format>$1 $2 $3</format></numberFormat>" +
                "  </availableFormats>" +
                "</territory>";
            XElement element = parseXmlString(xmlInput);
            PhoneMetadata.Builder metadata = new PhoneMetadata.Builder();
            BuildMetadataFromXml.LoadAvailableFormats(
                metadata, element, "0", "($1)", false /* NP not optional */);
            Assert.Equal(0, metadata.IntlNumberFormatCount);
        }

        [Fact]
        public void TestLoadAvailableFormatsHandlesMultipleNumberFormats()
        {
            String xmlInput =
                "<territory>" +
                "  <availableFormats>" +
                "    <numberFormat><format>$1 $2 $3</format></numberFormat>" +
                "    <numberFormat><format>$1-$2</format></numberFormat>" +
                "  </availableFormats>" +
                "</territory>";
            XElement element = parseXmlString(xmlInput);
            PhoneMetadata.Builder metadata = new PhoneMetadata.Builder();
            BuildMetadataFromXml.LoadAvailableFormats(
                metadata, element, "0", "($1)", false /* NP not optional */);
            Assert.Equal("$1 $2 $3", metadata.NumberFormatList[0].Format);
            Assert.Equal("$1-$2", metadata.NumberFormatList[1].Format);
        }

        [Fact]
        public void TestLoadInternationalFormatDoesNotSetIntlFormatWhenNA()
        {
            String xmlInput = "<numberFormat><intlFormat>NA</intlFormat></numberFormat>";
            XElement numberFormatElement = parseXmlString(xmlInput);
            PhoneMetadata.Builder metadata = new PhoneMetadata.Builder();
            String nationalFormat = "$1 $2";

            BuildMetadataFromXml.LoadInternationalFormat(metadata, numberFormatElement, nationalFormat);
            Assert.Equal(0, metadata.IntlNumberFormatCount);
        }

        // Tests setLeadingDigitsPatterns().
        [Fact]
        public void TestSetLeadingDigitsPatterns()
        {
            String xmlInput =
                "<numberFormat>" +
                "<leadingDigits>1</leadingDigits><leadingDigits>2</leadingDigits>" +
                "</numberFormat>";
            XElement numberFormatElement = parseXmlString(xmlInput);
            NumberFormat.Builder numberFormat = new NumberFormat.Builder();
            BuildMetadataFromXml.SetLeadingDigitsPatterns(numberFormatElement, numberFormat);

            Assert.Equal("1", numberFormat.LeadingDigitsPatternList[0]);
            Assert.Equal("2", numberFormat.LeadingDigitsPatternList[1]);
        }

        // Tests GetNationalPrefixFormattingRuleFromElement().
        [Fact]
        public void TestGetNationalPrefixFormattingRuleFromElement()
        {
            String xmlInput = "<territory nationalPrefixFormattingRule='$NP$FG'/>";
            XElement element = parseXmlString(xmlInput);
            Assert.Equal("0${1}",
                         BuildMetadataFromXml.GetNationalPrefixFormattingRuleFromElement(element, "0"));
        }

        // Tests getDomesticCarrierCodeFormattingRuleFromElement().
        [Fact]
        public void TestGetDomesticCarrierCodeFormattingRuleFromElement()
        {
            String xmlInput = "<territory carrierCodeFormattingRule='$NP$CC $FG'/>";
            XElement element = parseXmlString(xmlInput);
            // C#: the output regex differs from Java one
            Assert.Equal("0$CC ${1}",
                         BuildMetadataFromXml.GetDomesticCarrierCodeFormattingRuleFromElement(element,
                                                                                              "0"));
        }

        // Tests isValidNumberType().
        [Fact]
        public void TestIsValidNumberTypeWithInvalidInput()
        {
            Assert.False(BuildMetadataFromXml.IsValidNumberType("invalidType"));
        }

        // Tests ProcessPhoneNumberDescElement().
        [Fact]
        public void TestProcessPhoneNumberDescElementWithInvalidInput()
        {
            XElement territoryElement = parseXmlString("<territory/>");

            var phoneNumberDesc = BuildMetadataFromXml.ProcessPhoneNumberDescElement(
                null, territoryElement, "invalidType", false);
            Assert.Equal("NA", phoneNumberDesc.PossibleNumberPattern);
            Assert.Equal("NA", phoneNumberDesc.NationalNumberPattern);
        }

        [Fact]
        public void TestProcessPhoneNumberDescElementMergesWithGeneralDesc()
        {
            PhoneNumberDesc generalDesc = new PhoneNumberDesc.Builder()
                .SetPossibleNumberPattern("\\d{6}").Build();
            XElement territoryElement = parseXmlString("<territory><fixedLine/></territory>");

            var phoneNumberDesc = BuildMetadataFromXml.ProcessPhoneNumberDescElement(
                generalDesc, territoryElement, "fixedLine", false);
            Assert.Equal("\\d{6}", phoneNumberDesc.PossibleNumberPattern);
        }

        [Fact]
        public void TestProcessPhoneNumberDescElementOverridesGeneralDesc()
        {
            PhoneNumberDesc generalDesc = new PhoneNumberDesc.Builder()
                .SetPossibleNumberPattern("\\d{8}").Build();
            String xmlInput =
                "<territory><fixedLine>" +
                "  <possibleNumberPattern>\\d{6}</possibleNumberPattern>" +
                "</fixedLine></territory>";
            XElement territoryElement = parseXmlString(xmlInput);

            var phoneNumberDesc = BuildMetadataFromXml.ProcessPhoneNumberDescElement(
                generalDesc, territoryElement, "fixedLine", false);
            Assert.Equal("\\d{6}", phoneNumberDesc.PossibleNumberPattern);
        }

        [Fact]
        public void TestProcessPhoneNumberDescElementHandlesLiteBuild()
        {
            String xmlInput =
                "<territory><fixedLine>" +
                "  <exampleNumber>01 01 01 01</exampleNumber>" +
                "</fixedLine></territory>";
            XElement territoryElement = parseXmlString(xmlInput);
            var phoneNumberDesc = BuildMetadataFromXml.ProcessPhoneNumberDescElement(
                null, territoryElement, "fixedLine", true);
            Assert.Equal("", phoneNumberDesc.ExampleNumber);
        }

        [Fact]
        public void TestProcessPhoneNumberDescOutputsExampleNumberByDefault()
        {
            String xmlInput =
                "<territory><fixedLine>" +
                 "  <exampleNumber>01 01 01 01</exampleNumber>" +
                 "</fixedLine></territory>";
            XElement territoryElement = parseXmlString(xmlInput);

            var phoneNumberDesc = BuildMetadataFromXml.ProcessPhoneNumberDescElement(
                null, territoryElement, "fixedLine", false);
            Assert.Equal("01 01 01 01", phoneNumberDesc.ExampleNumber);
        }

        [Fact]
        public void TestProcessPhoneNumberDescRemovesWhiteSpacesInPatterns()
        {
            String xmlInput =
                "<territory><fixedLine>" +
                 "  <possibleNumberPattern>\t \\d { 6 } </possibleNumberPattern>" +
                 "</fixedLine></territory>";
            XElement countryElement = parseXmlString(xmlInput);

            var phoneNumberDesc = BuildMetadataFromXml.ProcessPhoneNumberDescElement(
                null, countryElement, "fixedLine", false);
            Assert.Equal("\\d{6}", phoneNumberDesc.PossibleNumberPattern);
        }

        // Tests LoadGeneralDesc().
        [Fact]
        public void TestLoadGeneralDescSetsSameMobileAndFixedLinePattern()
        {
            String xmlInput =
                "<territory countryCode=\"33\">" +
                "  <fixedLine><nationalNumberPattern>\\d{6}</nationalNumberPattern></fixedLine>" +
                "  <mobile><nationalNumberPattern>\\d{6}</nationalNumberPattern></mobile>" +
                "</territory>";
            XElement territoryElement = parseXmlString(xmlInput);
            PhoneMetadata.Builder metadata = new PhoneMetadata.Builder();
            // Should set sameMobileAndFixedPattern to true.
            BuildMetadataFromXml.LoadGeneralDesc(metadata, territoryElement, false);
            Assert.True(metadata.SameMobileAndFixedLinePattern);
        }

        [Fact]
        public void TestLoadGeneralDescSetsAllDescriptions()
        {
            String xmlInput =
                "<territory countryCode=\"33\">" +
                "  <fixedLine><nationalNumberPattern>\\d{1}</nationalNumberPattern></fixedLine>" +
                "  <mobile><nationalNumberPattern>\\d{2}</nationalNumberPattern></mobile>" +
                "  <pager><nationalNumberPattern>\\d{3}</nationalNumberPattern></pager>" +
                "  <tollFree><nationalNumberPattern>\\d{4}</nationalNumberPattern></tollFree>" +
                "  <premiumRate><nationalNumberPattern>\\d{5}</nationalNumberPattern></premiumRate>" +
                "  <sharedCost><nationalNumberPattern>\\d{6}</nationalNumberPattern></sharedCost>" +
                "  <personalNumber><nationalNumberPattern>\\d{7}</nationalNumberPattern></personalNumber>" +
                "  <voip><nationalNumberPattern>\\d{8}</nationalNumberPattern></voip>" +
                "  <uan><nationalNumberPattern>\\d{9}</nationalNumberPattern></uan>" +
                "  <shortCode><nationalNumberPattern>\\d{10}</nationalNumberPattern></shortCode>" +
                 "</territory>";
            XElement territoryElement = parseXmlString(xmlInput);
            PhoneMetadata.Builder metadata = new PhoneMetadata.Builder();
            BuildMetadataFromXml.LoadGeneralDesc(metadata, territoryElement, false);
            Assert.Equal("\\d{1}", metadata.FixedLine.NationalNumberPattern);
            Assert.Equal("\\d{2}", metadata.Mobile.NationalNumberPattern);
            Assert.Equal("\\d{3}", metadata.Pager.NationalNumberPattern);
            Assert.Equal("\\d{4}", metadata.TollFree.NationalNumberPattern);
            Assert.Equal("\\d{5}", metadata.PremiumRate.NationalNumberPattern);
            Assert.Equal("\\d{6}", metadata.SharedCost.NationalNumberPattern);
            Assert.Equal("\\d{7}", metadata.PersonalNumber.NationalNumberPattern);
            Assert.Equal("\\d{8}", metadata.Voip.NationalNumberPattern);
            Assert.Equal("\\d{9}", metadata.Uan.NationalNumberPattern);
        }
    }
}