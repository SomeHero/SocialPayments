using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Drawing;

namespace SocialPayments.DomainServices.UnitTests
{
    /// <summary>
    /// Summary description for OCRCheckTest
    /// </summary>
    [TestClass]
    public class OCRCheckTest
    {
        public OCRCheckTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestMethod1()
        {
            Bitmap image = new Bitmap(@"c:\data\photo.jpg");
            tessnet2.Tesseract ocr = new tessnet2.Tesseract();
            ocr.SetVariable("tessedit_char_whitelist", "0123456789"); // If digit only
            ocr.Init(@"c:\data", "eng", false); // To use correct tessdata
            List<tessnet2.Word> result = ocr.DoOCR(image, Rectangle.Empty);
            foreach (tessnet2.Word word in result)
                Console.WriteLine("{0} : {1}", word.Confidence, word.Text);
        }
        public class Ocr
        {
            public void DumpResult(List<tessnet2.Word> result)
            {
                foreach (tessnet2.Word word in result)
                    Console.WriteLine("{0} : {1}", word.Confidence, word.Text);
            }

            public List<tessnet2.Word> DoOCRNormal(Bitmap image, string lang)
            {
                tessnet2.Tesseract ocr = new tessnet2.Tesseract();
                ocr.Init(null, lang, false);
                List<tessnet2.Word> result = ocr.DoOCR(image, Rectangle.Empty);
                DumpResult(result);
                return result;
            }

            ManualResetEvent m_event;

            public void DoOCRMultiThred(Bitmap image, string lang)
            {
                tessnet2.Tesseract ocr = new tessnet2.Tesseract();
                ocr.Init(null, lang, false);
                // If the OcrDone delegate is not null then this'll be the multithreaded version
                ocr.OcrDone = new tessnet2.Tesseract.OcrDoneHandler(Finished);
                // For event to work, must use the multithreaded version
                ocr.ProgressEvent += new tessnet2.Tesseract.ProgressHandler(ocr_ProgressEvent);
                m_event = new ManualResetEvent(false);
                ocr.DoOCR(image, Rectangle.Empty);
                // Wait here it's finished
                m_event.WaitOne();
            }

            public void Finished(List<tessnet2.Word> result)
            {
                DumpResult(result);
                m_event.Set();
            }

            void ocr_ProgressEvent(int percent)
            {
                Console.WriteLine("{0}% progression", percent);
            }
        }
    }
}
