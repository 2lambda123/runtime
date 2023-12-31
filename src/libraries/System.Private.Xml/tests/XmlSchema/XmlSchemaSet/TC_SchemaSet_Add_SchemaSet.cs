// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using System.Xml.Schema;
using Xunit;
using Xunit.Abstractions;

namespace System.Xml.XmlSchemaTests
{
    public class TC_SchemaSet_Add_SchemaSet : TC_SchemaSetBase
    {
        private ITestOutputHelper _output;

        public TC_SchemaSet_Add_SchemaSet(ITestOutputHelper output)
        {
            _output = output;
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v1 - sc = null", Priority = 0)]
        public void v1()
        {
            try
            {
                XmlSchemaSet sc = new XmlSchemaSet();
                sc.Add((XmlSchemaSet)null);
            }
            catch (ArgumentNullException)
            {
                return;
            }

            Assert.Fail();
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v2 - sc = empty SchemaSet", Priority = 0)]
        public void v2()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            XmlSchemaSet scnew = new XmlSchemaSet();
            sc.Add(scnew);

            Assert.Equal(0, sc.Count);

            return;
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v3 - sc = non empty SchemaSet, add with duplicate schemas", Priority = 0)]
        public void v3()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            XmlSchemaSet scnew = new XmlSchemaSet();

            sc.Add("xsdauthor", TestData._XsdAuthor);
            scnew.Add("xsdauthor", TestData._XsdAuthor);

            sc.Add(scnew);
            // adding schemaset with same schema should be ignored
            Assert.Equal(1, sc.Count);
            return;
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v4 - sc = self", Priority = 0)]
        public void v4()
        {
            XmlSchemaSet sc = new XmlSchemaSet();

            sc.Add("xsdauthor", TestData._XsdAuthor);
            sc.Add(sc);

            Assert.Equal(1, sc.Count);

            return;
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v5 - sc = scnew, scnew as some duplicate but some unique schemas")]
        public void v5()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            XmlSchemaSet scnew = new XmlSchemaSet();

            sc.Add("xsdauthor", TestData._XsdAuthor);
            sc.Add(null, TestData._XsdNoNs);

            scnew.Add(null, TestData._XsdNoNs);
            scnew.Add(null, TestData._FileXSD1);
            sc.Add(scnew);
            Assert.Equal(3, sc.Count);
            return;
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v6 - sc = add second set with all new schemas")]
        public void v6()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            XmlSchemaSet scnew = new XmlSchemaSet();

            sc.Add("xsdauthor", TestData._XsdAuthor);
            sc.Add(null, TestData._XsdNoNs);
            scnew.Add(null, TestData._FileXSD1);
            scnew.Add(null, TestData._FileXSD2);
            sc.Add(scnew);
            Assert.Equal(4, sc.Count);
            return;
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v7 - sc = add second set with a conflicting schema")]
        public void v7()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            XmlSchemaSet scnew = new XmlSchemaSet();

            sc.Add("xsdauthor", TestData._XsdAuthor);
            sc.Add(null, TestData._XsdNoNs);
            scnew.Add(null, TestData._FileXSD1);
            scnew.Add(null, TestData._XsdAuthorDup); // this conflicts with _XsdAuthor
            sc.Add(scnew);
            Assert.False(sc.IsCompiled);
            Assert.Equal(4, sc.Count); //ok
            try
            {
                sc.Compile(); // should fail
            }
            catch (XmlSchemaException)
            {
                return;
            }
            Assert.Fail();
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v8 - sc = add second set with a conflicting schema to compiled set")]
        public void v8()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            XmlSchemaSet scnew = new XmlSchemaSet();

            sc.Add("xsdauthor", TestData._XsdAuthor);
            sc.Add(null, TestData._XsdNoNs);
            sc.Compile();
            Assert.True(sc.IsCompiled);
            scnew.Add(null, TestData._FileXSD1);
            scnew.Add(null, TestData._XsdAuthorDup); // this conflicts with _XsdAuthor
            sc.Add(scnew);
            Assert.False(sc.IsCompiled);
            Assert.Equal(4, sc.Count); //ok
            try
            {
                sc.Compile(); // should fail
            }
            catch (XmlSchemaException)
            {
                return;
            }
            Assert.Fail();
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v9 - sc = add compiled second set with a conflicting schema to compiled set")]
        public void v9()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            XmlSchemaSet scnew = new XmlSchemaSet();

            sc.Add("xsdauthor", TestData._XsdAuthor);
            sc.Add(null, TestData._XsdNoNs);
            sc.Compile();
            Assert.True(sc.IsCompiled);
            scnew.Add(null, TestData._FileXSD1);
            scnew.Add(null, TestData._XsdAuthorDup); // this conflicts with _XsdAuthor
            scnew.Compile();

            try
            {
                sc.Add(scnew);
            }
            catch (XmlSchemaException)
            {
                return;
            }
            Assert.Fail();
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v10 - set1 added to set2 with set1 containing an invalid schema")]
        public void v10()
        {
            XmlSchemaSet schemaSet1 = new XmlSchemaSet();
            XmlSchemaSet schemaSet2 = new XmlSchemaSet();

            XmlSchema schema1 = XmlSchema.Read(new StreamReader(new FileStream(TestData._XsdAuthor, FileMode.Open, FileAccess.Read)), null);
            XmlSchema schema2 = XmlSchema.Read(new StreamReader(new FileStream(TestData._XsdNoNs, FileMode.Open, FileAccess.Read)), null);

            schemaSet1.Add(schema1);
            schemaSet1.Add(schema2); // added two schemas

            XmlSchemaElement elem = new XmlSchemaElement();
            schema1.Items.Add(elem);  // make the first schema dirty

            //the following throws an exception
            try
            {
                schemaSet2.Add(schemaSet1);
                // shound not reach here
            }
            catch (XmlSchemaException)
            {
                Assert.Equal(0, schemaSet2.Count); // no schema should be added
                Assert.Equal(2, schemaSet1.Count); // no schema should be added
                Assert.False(schemaSet2.IsCompiled); // no schema should be added
                Assert.False(schemaSet1.IsCompiled); // no schema should be added
                return;
            }

            Assert.Equal(0, schemaSet2.Count); // no schema should be added
            Assert.Fail();
        }

        [Fact]
        //[Variation(Desc = "v11 - Add three XmlSchema to Set1 then add Set1 to uncompiled Set2")]
        public void v11()
        {
            XmlSchemaSet schemaSet1 = new XmlSchemaSet();
            XmlSchemaSet schemaSet2 = new XmlSchemaSet();
            XmlSchema schema1 = XmlSchema.Read(new StreamReader(new FileStream(TestData._XsdAuthor, FileMode.Open, FileAccess.Read)), null);
            XmlSchema schema2 = XmlSchema.Read(new StreamReader(new FileStream(TestData._XsdNoNs, FileMode.Open, FileAccess.Read)), null);
            XmlSchema schema3 = XmlSchema.Read(new StreamReader(new FileStream(TestData._FileXSD1, FileMode.Open, FileAccess.Read)), null);

            schemaSet1.Add(schema1);
            schemaSet1.Add(schema2); // added two schemas
            schemaSet1.Add(schema3); // added third
            schemaSet1.Compile();

            //the following throws an exception
            try
            {
                schemaSet2.Add(schemaSet1);
                Assert.Equal(3, schemaSet1.Count); // no schema should be added
                Assert.Equal(3, schemaSet2.Count); // no schema should be added
                Assert.True(schemaSet1.IsCompiled); // no schema should be added
                schemaSet2.Compile();
                Assert.True(schemaSet2.IsCompiled); // no schema should be added
                // shound not reach here
            }
            catch (XmlSchemaException)
            {
                Assert.Fail();
            }
            return;
        }
    }
}
