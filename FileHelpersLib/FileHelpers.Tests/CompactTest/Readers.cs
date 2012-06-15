using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using FileHelpersTests;
using NUnit.Framework;

namespace FileHelpers.CompactTest
{
    [TestFixture]
    public class Readers
    {
        FileHelperEngine engine;
        FileHelperAsyncEngine asyncEngine;

        [Test]
        public void ReadFile()
        {
            engine = new FileHelperEngine(typeof(SampleType));

            SampleType[] res;
            res = (SampleType[]) Common.ReadTest(engine, @"Good\test1.txt");

            Assert.AreEqual(4, res.Length);
            Assert.AreEqual(4, engine.TotalRecords);
            Assert.AreEqual(0, engine.ErrorManager.ErrorCount);

            Assert.AreEqual(new DateTime(1314, 12, 11), res[0].Field1);
            Assert.AreEqual("901", res[0].Field2);
            Assert.AreEqual(234, res[0].Field3);

            Assert.AreEqual(new DateTime(1314, 11, 10), res[1].Field1);
            Assert.AreEqual("012", res[1].Field2);
            Assert.AreEqual(345, res[1].Field3);

        }


        [Test]
        public void AsyncRead()
        {
            asyncEngine = new FileHelperAsyncEngine(typeof(SampleType));

            SampleType rec1, rec2;

            Common.BeginReadTest(asyncEngine, @"Good\test1.txt");

            rec1 = (SampleType)asyncEngine.ReadNext();
            Assert.IsNotNull(rec1);
            rec2 = (SampleType)asyncEngine.ReadNext();
            Assert.IsNotNull(rec1);

            Assert.IsTrue(rec1 != rec2);

            rec1 = (SampleType)asyncEngine.ReadNext();
            Assert.IsNotNull(rec2);
            rec1 = (SampleType)asyncEngine.ReadNext();
            Assert.IsNotNull(rec2);

            Assert.IsTrue(rec1 != rec2);

            Assert.AreEqual(0, asyncEngine.ErrorManager.ErrorCount);

            asyncEngine.Close();
        }

        [Test]
        public void AsyncReadMoreAndMore()
        {
            asyncEngine = new FileHelperAsyncEngine(typeof(SampleType));

            SampleType rec1;

            Common.BeginReadTest(asyncEngine, @"Good\test1.txt");

            rec1 = (SampleType)asyncEngine.ReadNext();
            rec1 = (SampleType)asyncEngine.ReadNext();
            rec1 = (SampleType)asyncEngine.ReadNext();
            rec1 = (SampleType)asyncEngine.ReadNext();
            rec1 = (SampleType)asyncEngine.ReadNext();

            Assert.IsTrue(rec1 == null);

            rec1 = (SampleType)asyncEngine.ReadNext();
            Assert.AreEqual(0, asyncEngine.ErrorManager.ErrorCount);

            asyncEngine.Close();
        }


        [Test]
        public void AsyncRead2()
        {
            SampleType rec1;

            asyncEngine = new FileHelperAsyncEngine(typeof(SampleType));

            Common.BeginReadTest(asyncEngine, @"Good\test1.txt");
            int lineAnt = asyncEngine.LineNumber;
            while (asyncEngine.ReadNext() != null)
            {
                rec1 = (SampleType)asyncEngine.LastRecord;
                Assert.IsNotNull(rec1);
                Assert.AreEqual(lineAnt + 1, asyncEngine.LineNumber);
                lineAnt = asyncEngine.LineNumber;
            }

            Assert.AreEqual(4, asyncEngine.TotalRecords);
            Assert.AreEqual(0, asyncEngine.ErrorManager.ErrorCount);

            asyncEngine.Close();

        }

        [Test]
        public void AsyncReadEnumerable()
        {
            asyncEngine = new FileHelperAsyncEngine(typeof(SampleType));

            Common.BeginReadTest(asyncEngine, @"Good\test1.txt");
            int lineAnt = asyncEngine.LineNumber;

            foreach (SampleType rec1 in asyncEngine)
            {
                Assert.IsNotNull(rec1);
                Assert.AreEqual(lineAnt + 1, asyncEngine.LineNumber);
                lineAnt = asyncEngine.LineNumber;
            }

            Assert.AreEqual(4, asyncEngine.TotalRecords);
            Assert.AreEqual(0, asyncEngine.ErrorManager.ErrorCount);

            asyncEngine.Close();
        }

        [Test]
        [ExpectedException(typeof(FileHelpersException))]
        public void AsyncReadEnumerableBad()
        {
            asyncEngine = new FileHelperAsyncEngine(typeof(SampleType));

            foreach (SampleType rec1 in asyncEngine)
            {
                rec1.ToString();
            }
            asyncEngine.Close();
        }

        [Test]
        public void AsyncReadEnumerable2()
        {
            using (asyncEngine = new FileHelperAsyncEngine(typeof(SampleType)))
            {
                Common.BeginReadTest(asyncEngine, @"Good\test1.txt");
                int lineAnt = asyncEngine.LineNumber;

                foreach (SampleType rec1 in asyncEngine)
                {
                    Assert.IsNotNull(rec1);
                    Assert.AreEqual(lineAnt + 1, asyncEngine.LineNumber);
                    lineAnt = asyncEngine.LineNumber;
                }

            }

            Assert.AreEqual(4, asyncEngine.TotalRecords);
            Assert.AreEqual(0, asyncEngine.ErrorManager.ErrorCount);
            asyncEngine.Close();
        }

        [Test]
        public void AsyncReadEnumerableAutoDispose()
        {
            asyncEngine = new FileHelperAsyncEngine(typeof(SampleType));
            Common.BeginReadTest(asyncEngine, @"Good\test1.txt");

            asyncEngine.ReadNext();
            asyncEngine.ReadNext();

            asyncEngine.Close();
        }

        [Test]
        public void ReadStream()
        {
            string data = "11121314901234\n"+
                "10111314012345\n" + 
                "11101314123456\n" + 
                "10101314234567\n" ;

            engine = new FileHelperEngine(typeof(SampleType));

            SampleType[] res;
            res = (SampleType[])engine.ReadStream(new StringReader(data));

            Assert.AreEqual(4, res.Length);
            Assert.AreEqual(4, engine.TotalRecords);
            Assert.AreEqual(0, engine.ErrorManager.ErrorCount);

            Assert.AreEqual(new DateTime(1314, 12, 11), res[0].Field1);
            Assert.AreEqual("901", res[0].Field2);
            Assert.AreEqual(234, res[0].Field3);

            Assert.AreEqual(new DateTime(1314, 11, 10), res[1].Field1);
            Assert.AreEqual("012", res[1].Field2);
            Assert.AreEqual(345, res[1].Field3);

        }

        [Test]
        public void ReadString()
        {
            string data = "11121314901234\n" +
                "10111314012345\n" +
                "11101314123456\n" + 
                "10101314234567\n" ;

            engine = new FileHelperEngine(typeof(SampleType));

            SampleType[] res;
            res = (SampleType[])engine.ReadString(data);

            Assert.AreEqual(4, res.Length);
            Assert.AreEqual(4, engine.TotalRecords);
            Assert.AreEqual(0, engine.ErrorManager.ErrorCount);

            Assert.AreEqual(new DateTime(1314, 12, 11), res[0].Field1);
            Assert.AreEqual("901", res[0].Field2);
            Assert.AreEqual(234, res[0].Field3);

            Assert.AreEqual(new DateTime(1314, 11, 10), res[1].Field1);
            Assert.AreEqual("012", res[1].Field2);
            Assert.AreEqual(345, res[1].Field3);

        }


       

        [Test]
        public void ReadEmpty()
        {
            string data = "";

            engine = new FileHelperEngine(typeof(SampleType));

            SampleType[] res;
            res = (SampleType[])engine.ReadStream(new StringReader(data));

            Assert.AreEqual(0, res.Length);
            Assert.AreEqual(0, engine.TotalRecords);
            Assert.AreEqual(0, engine.ErrorManager.ErrorCount);

        }

        [Test]
        public void ReadEmptyStream()
        {
            engine = new FileHelperEngine(typeof(SampleType));

            SampleType[] res;
            res = (SampleType[])Common.ReadTest(engine, @"Good\TestEmpty.txt");

            Assert.AreEqual(0, res.Length);
            Assert.AreEqual(0, engine.TotalRecords);
            Assert.AreEqual(0, engine.ErrorManager.ErrorCount);

        }


        [Test]
        public void ReadAsyncFieldIndex()
        {
            string data = "11121314901234\n" +
                "10111314012345\n" +
                "11101314123456\n" +
                "10101314234567\n" ;

            asyncEngine = new FileHelperAsyncEngine(typeof(SampleType));
            asyncEngine.BeginReadString(data);

            foreach (SampleType rec in asyncEngine)
            {
                Assert.AreEqual(rec.Field1, asyncEngine[0]);
                Assert.AreEqual(rec.Field2, asyncEngine[1]);
                Assert.AreEqual(rec.Field3, asyncEngine[2]);

                Assert.AreEqual(rec.Field1, asyncEngine["Field1"]);
                Assert.AreEqual(rec.Field2, asyncEngine["Field2"]);
                Assert.AreEqual(rec.Field3, asyncEngine["Field3"]);

            }

            asyncEngine.Close();


        }

    }
}
