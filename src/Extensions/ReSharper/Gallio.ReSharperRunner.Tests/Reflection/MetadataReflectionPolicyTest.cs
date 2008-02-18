// Copyright 2008 MbUnit Project - http://www.mbunit.com/
// Portions Copyright 2000-2004 Jonathan De Halleux, Jamie Cansdale
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

extern alias MbUnit2;

using System.Reflection;
using Gallio.ReSharperRunner.Reflection;
using Gallio.Tests.Reflection;
using JetBrains.Metadata.Reader.API;
using MbUnit2::MbUnit.Framework;

namespace Gallio.ReSharperRunner.Tests.Reflection
{
    [TestFixture]
    [TestsOn(typeof(MetadataReflectionPolicy))]
    public class MetadataReflectionPolicyTest : BaseReflectionPolicyTest
    {
        private MetadataLoader loader;
        private MetadataReflectionPolicy reflectionPolicy;

        public override void SetUp()
        {
            base.SetUp();
            WrapperAssert.SupportsSpecialFeatures = false;

            loader = new MetadataLoader(BuiltInMetadataAssemblyResolver.Instance);

            Assembly assembly = GetType().Assembly;
            IMetadataAssembly metadataAssembly = loader.Load(assembly.GetName(), delegate { return true; });

            reflectionPolicy = new MetadataReflectionPolicy(metadataAssembly, null);
        }

        public override void TearDown()
        {
            if (loader != null)
            {
                loader.Dispose();
                loader = null;
            }

            base.TearDown();
        }

        protected override Gallio.Reflection.IReflectionPolicy ReflectionPolicy
        {
            get { return reflectionPolicy; }
        }

        [Test]
        public void WrapNullReturnsNull()
        {
            Assert.IsNull(reflectionPolicy.Wrap((IMetadataAssembly)null));
        }
    }
}