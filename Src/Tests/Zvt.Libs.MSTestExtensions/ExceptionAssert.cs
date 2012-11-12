using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Zvt.Libs.MSTestExtensions
{
    /*
     * Based on the code of https://github.com/bbraithwaite/MSTestExtensions
    */

    public static class AssertException
    {
        public static void Throws<T>(
            Action task, 
            string expectedMessage = null,
            ExceptionMessageCompareOptions options = ExceptionMessageCompareOptions.None
            ) 
            where T : Exception
        {
            try
            {
                task();
            }
            catch (Exception ex)
            {
                AssertExceptionType<T>(ex);
                AssertExceptionMessage(ex, expectedMessage, options);
                return;
            }

            if (typeof(T).Equals(new Exception().GetType()))
            {
                Assert.Fail("Expected exception but no exception was thrown.");
            }
            else
            {
                Assert.Fail(string.Format("Expected exception of type {0} but no exception was thrown.", typeof(T)));
            }
        }

        public static void Throws(
            Action task,
            string expectedMessage = null,
            ExceptionMessageCompareOptions options = ExceptionMessageCompareOptions.None
            )
        {
            Throws<Exception>(task, expectedMessage, options);
        }

        private static void AssertExceptionType<T>(Exception ex)
        {
            Assert.IsInstanceOfType(ex, typeof(T), "Expected exception type failed.");
        }

        private static void AssertExceptionMessage(Exception ex, string expectedMessage, ExceptionMessageCompareOptions options)
        {
            if (!string.IsNullOrEmpty(expectedMessage))
            {
                switch (options)
                {
                    case ExceptionMessageCompareOptions.Exact:
                        Assert.AreEqual(ex.Message.ToUpper(), expectedMessage.ToUpper(), "Expected exception message failed.");
                        break;
                    case ExceptionMessageCompareOptions.Contains:
                        Assert.IsTrue(ex.Message.Contains(expectedMessage), string.Format("Expected exception message does not contain <{0}>.", expectedMessage));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("options");
                }

            }
        }
    }
}