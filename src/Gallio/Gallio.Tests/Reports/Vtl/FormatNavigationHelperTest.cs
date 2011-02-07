// Copyright 2005-2010 Gallio Project - http://www.gallio.org/
// Portions Copyright 2000-2004 Jonathan de Halleux
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Xml;
using Gallio.Common.Markup;
using Gallio.Common.Policies;
using Gallio.Reports;
using Gallio.Reports.Vtl;
using Gallio.Runtime.ProgressMonitoring;
using Gallio.Runtime;
using Gallio.Framework;
using Gallio.Common.Reflection;
using Gallio.Runner.Reports;
using Gallio.Tests;
using MbUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;
using Gallio.Common.Collections;
using Gallio.Runner.Reports.Schema;
using NVelocity.App;
using NVelocity.Runtime;
using System.Reflection;
using Gallio.Common.Markup.Tags;
using Gallio.Model.Schema;

namespace Gallio.Tests.Reports.Vtl
{
    [TestFixture]
    [TestsOn(typeof(FormatNavigationHelper))]
    public class FormatNavigationHelperTest
    {
        [Test]
        [Row("name1", "value1")]
        [Row("name2", "value2")]
        [Row("name3", "value3")]
        [Row("inexisting", "", Description = "Inexisting attribute should return empty value.")]
        public void GetMarkerAttributeValue(string searched, string expectedValue)
        {
            // Create sample marker tag.
            var tag = new MarkerTag(Marker.Label);
            tag.Attributes.Add(new MarkerTag.Attribute("name1", "value1"));
            tag.Attributes.Add(new MarkerTag.Attribute("name2", "value2"));
            tag.Attributes.Add(new MarkerTag.Attribute("name3", "value3"));

            // Find attribute value.
            var helper = new FormatNavigationHelper();
            string value = helper.GetMarkerAttributeValue(tag, searched);
            Assert.AreEqual(expectedValue, value);
        }

        private TestData CreateFakeTest(string id, params TestData[] children)
        {
            var test = new TestData(id, "Name-" + id, "FullName-" + id);
            test.Children.AddRange(children);
            return test;
        }

        [Test]
        public void GetSelfAndAncestorIds()
        {
            // Root 
            //   +- Child1
            //   +- Child2
            //   +- Child3
            //        +- Child31
            //        +- Child32
            //             +- Child321
            //             +- Child322
            //             +- Child323
            //             +- Child324
            //        +- Child33
            //             +- Child331
            //             +- Child332
            //             +- Child333

            var child1 = CreateFakeTest("1");
            var child2 = CreateFakeTest("2");
            var child331 = CreateFakeTest("331");
            var child332 = CreateFakeTest("332");
            var child333 = CreateFakeTest("333");
            var child33 = CreateFakeTest("33", child331, child332, child333);
            var child321 = CreateFakeTest("321");
            var child322 = CreateFakeTest("322");
            var child323 = CreateFakeTest("323");
            var child324 = CreateFakeTest("324");
            var child32 = CreateFakeTest("32", child321, child322, child323, child324);
            var child31 = CreateFakeTest("31");
            var child3 = CreateFakeTest("3", child31, child32, child33);
            var root = CreateFakeTest("Root", child1, child2, child3);

            var helper = new FormatNavigationHelper();
            helper.BuildParentMap(root);
            Assert.AreElementsEqual(new[] { "322", "32", "3", "Root" }, helper.GetSelfAndAncestorIds(child322.Id));
            Assert.AreElementsEqual(new[] { "31", "3", "Root" }, helper.GetSelfAndAncestorIds(child31.Id));
            Assert.AreElementsEqual(new[] { "2", "Root" }, helper.GetSelfAndAncestorIds(child2.Id));
            Assert.AreElementsEqual(new[] { "Root" }, helper.GetSelfAndAncestorIds(root.Id));
        }
    }
}