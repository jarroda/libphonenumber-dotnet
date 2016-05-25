/*
 * Copyright (C) 2011 The Libphonenumber Authors
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
using Xunit;

namespace PhoneNumbers.Test
{
    /**
     * Unit Tests for ShortNumberUtil.java
     *
     * @author Shaopeng Jia
     */
    public class ShortNumberUtilTest: TestMetadataTestCase
    {
        private ShortNumberUtil shortUtil;
        
        public ShortNumberUtilTest()
        {
            shortUtil = new ShortNumberUtil(phoneUtil);
        }

        [Fact]
        public void TestConnectsToEmergencyNumber_US()
        {
            Assert.True(shortUtil.ConnectsToEmergencyNumber("911", RegionCode.US));
            Assert.True(shortUtil.ConnectsToEmergencyNumber("119", RegionCode.US));
            Assert.False(shortUtil.ConnectsToEmergencyNumber("999", RegionCode.US));
        }

        [Fact]
        public void TestConnectsToEmergencyNumberLongNumber_US()
        {
            Assert.True(shortUtil.ConnectsToEmergencyNumber("9116666666", RegionCode.US));
            Assert.True(shortUtil.ConnectsToEmergencyNumber("1196666666", RegionCode.US));
            Assert.False(shortUtil.ConnectsToEmergencyNumber("9996666666", RegionCode.US));
        }

        [Fact]
        public void TestConnectsToEmergencyNumberWithFormatting_US()
        {
            Assert.True(shortUtil.ConnectsToEmergencyNumber("9-1-1", RegionCode.US));
            Assert.True(shortUtil.ConnectsToEmergencyNumber("1-1-9", RegionCode.US));
            Assert.False(shortUtil.ConnectsToEmergencyNumber("9-9-9", RegionCode.US));
        }

        [Fact]
        public void TestConnectsToEmergencyNumberWithPlusSign_US()
        {
            Assert.False(shortUtil.ConnectsToEmergencyNumber("+911", RegionCode.US));
            Assert.False(shortUtil.ConnectsToEmergencyNumber("\uFF0B911", RegionCode.US));
            Assert.False(shortUtil.ConnectsToEmergencyNumber(" +911", RegionCode.US));
            Assert.False(shortUtil.ConnectsToEmergencyNumber("+119", RegionCode.US));
            Assert.False(shortUtil.ConnectsToEmergencyNumber("+999", RegionCode.US));
        }

        [Fact]
        public void TestConnectsToEmergencyNumber_BR()
        {
            Assert.True(shortUtil.ConnectsToEmergencyNumber("911", RegionCode.BR));
            Assert.True(shortUtil.ConnectsToEmergencyNumber("190", RegionCode.BR));
            Assert.False(shortUtil.ConnectsToEmergencyNumber("999", RegionCode.BR));
        }

        [Fact]
        public void TestConnectsToEmergencyNumberLongNumber_BR()
        {
            // Brazilian emergency numbers don't work when additional digits are appended.
            Assert.False(shortUtil.ConnectsToEmergencyNumber("9111", RegionCode.BR));
            Assert.False(shortUtil.ConnectsToEmergencyNumber("1900", RegionCode.BR));
            Assert.False(shortUtil.ConnectsToEmergencyNumber("9996", RegionCode.BR));
        }

        [Fact]
        public void TestConnectsToEmergencyNumber_AO()
        {
            // Angola doesn't have any metadata for emergency numbers in the Test metadata.
            Assert.False(shortUtil.ConnectsToEmergencyNumber("911", RegionCode.AO));
            Assert.False(shortUtil.ConnectsToEmergencyNumber("222123456", RegionCode.AO));
            Assert.False(shortUtil.ConnectsToEmergencyNumber("923123456", RegionCode.AO));
        }

        [Fact]
        public void TestConnectsToEmergencyNumber_ZW()
        {
            // Zimbabwe doesn't have any metadata in the Test metadata.
            Assert.False(shortUtil.ConnectsToEmergencyNumber("911", RegionCode.ZW));
            Assert.False(shortUtil.ConnectsToEmergencyNumber("01312345", RegionCode.ZW));
            Assert.False(shortUtil.ConnectsToEmergencyNumber("0711234567", RegionCode.ZW));
        }

        [Fact]
        public void TestIsEmergencyNumber_US()
        {
            Assert.True(shortUtil.IsEmergencyNumber("911", RegionCode.US));
            Assert.True(shortUtil.IsEmergencyNumber("119", RegionCode.US));
            Assert.False(shortUtil.IsEmergencyNumber("999", RegionCode.US));
        }

        [Fact]
        public void TestIsEmergencyNumberLongNumber_US()
        {
            Assert.False(shortUtil.IsEmergencyNumber("9116666666", RegionCode.US));
            Assert.False(shortUtil.IsEmergencyNumber("1196666666", RegionCode.US));
            Assert.False(shortUtil.IsEmergencyNumber("9996666666", RegionCode.US));
        }

        [Fact]
        public void TestIsEmergencyNumberWithFormatting_US()
        {
            Assert.True(shortUtil.IsEmergencyNumber("9-1-1", RegionCode.US));
            Assert.True(shortUtil.IsEmergencyNumber("*911", RegionCode.US));
            Assert.True(shortUtil.IsEmergencyNumber("1-1-9", RegionCode.US));
            Assert.True(shortUtil.IsEmergencyNumber("*119", RegionCode.US));
            Assert.False(shortUtil.IsEmergencyNumber("9-9-9", RegionCode.US));
            Assert.False(shortUtil.IsEmergencyNumber("*999", RegionCode.US));
        }

        [Fact]
        public void TestIsEmergencyNumberWithPlusSign_US()
        {
            Assert.False(shortUtil.IsEmergencyNumber("+911", RegionCode.US));
            Assert.False(shortUtil.IsEmergencyNumber("\uFF0B911", RegionCode.US));
            Assert.False(shortUtil.IsEmergencyNumber(" +911", RegionCode.US));
            Assert.False(shortUtil.IsEmergencyNumber("+119", RegionCode.US));
            Assert.False(shortUtil.IsEmergencyNumber("+999", RegionCode.US));
        }

        [Fact]
        public void TestIsEmergencyNumber_BR()
        {
            Assert.True(shortUtil.IsEmergencyNumber("911", RegionCode.BR));
            Assert.True(shortUtil.IsEmergencyNumber("190", RegionCode.BR));
            Assert.False(shortUtil.IsEmergencyNumber("999", RegionCode.BR));
        }

        [Fact]
        public void TestIsEmergencyNumberLongNumber_BR()
        {
            Assert.False(shortUtil.IsEmergencyNumber("9111", RegionCode.BR));
            Assert.False(shortUtil.IsEmergencyNumber("1900", RegionCode.BR));
            Assert.False(shortUtil.IsEmergencyNumber("9996", RegionCode.BR));
        }

        [Fact]
        public void TestIsEmergencyNumber_AO()
        {
            // Angola doesn't have any metadata for emergency numbers in the Test metadata.
            Assert.False(shortUtil.IsEmergencyNumber("911", RegionCode.AO));
            Assert.False(shortUtil.IsEmergencyNumber("222123456", RegionCode.AO));
            Assert.False(shortUtil.IsEmergencyNumber("923123456", RegionCode.AO));
        }

        [Fact]
        public void TestIsEmergencyNumber_ZW()
        {
            // Zimbabwe doesn't have any metadata in the Test metadata.
            Assert.False(shortUtil.IsEmergencyNumber("911", RegionCode.ZW));
            Assert.False(shortUtil.IsEmergencyNumber("01312345", RegionCode.ZW));
            Assert.False(shortUtil.IsEmergencyNumber("0711234567", RegionCode.ZW));
        }
    }
}
