using NUnit.Framework;

namespace Assignment
{
    public class Assignment_Testcase
    {
        private StudentSolution assignment;

        [SetUp]
        public void Setup()
        {
            // Use StudentSolution as the test subject
            assignment = new StudentSolution();
        }

        #region Lecture

        [Category("Lecture")]
        [Test]
        public void Test_LCT01_StackSyntax()
        {
            var actual = assignment.LCT01_StackSyntax();
            Assert.AreEqual(3, actual.Count, "Count before Pop");
            Assert.AreEqual(3, actual.Popped, "Pop returns the last pushed value");
            Assert.AreEqual(2, actual.Peek, "Peek returns the next value");
            Assert.AreEqual(2, actual.CountAfterPeek, "Peek does not remove a value");
        }

        [Category("Lecture")]
        [Test]
        public void Test_LCT02_QueueSyntax()
        {
            var actual = assignment.LCT02_QueueSyntax();
            Assert.AreEqual(3, actual.Count, "Count before Dequeue");
            Assert.AreEqual(1, actual.Dequeued, "Dequeue returns the first enqueued value");
            Assert.AreEqual(2, actual.Peek, "Peek returns the next value");
            Assert.AreEqual(2, actual.CountAfterPeek, "Peek does not remove a value");
        }

        [Category("Lecture")]
        [Test]
        public void Test_LCT03_ActionStack()
        {
            string[] actual = assignment.LCT03_ActionStack();
            CollectionAssert.AreEqual(new[] { "Action 3", "Action 2", "Action 1" }, actual);
        }

        [Category("Lecture")]
        [Test]
        public void Test_LCT04_ActionQueue()
        {
            string[] actual = assignment.LCT04_ActionQueue();
            CollectionAssert.AreEqual(new[] { "Action 1", "Action 2", "Action 3" }, actual);
        }

        #endregion

        #region Assignment

        [Category("Assignment")]
        [TestCase("hello", "olleh", TestName = "ASN01_ReverseString_Hello", Description = "Reverse hello")]
        [TestCase("world", "dlrow", TestName = "ASN01_ReverseString_World", Description = "Reverse world")]
        [TestCase("a", "a", TestName = "ASN01_ReverseString_Single", Description = "Reverse single char")]
        [TestCase("", "", TestName = "ASN01_ReverseString_Empty", Description = "Reverse empty string")]
        [TestCase("12345", "54321", TestName = "ASN01_ReverseString_Numbers", Description = "Reverse numbers")]
        public void Test_ASN01_ReverseString(string str, string expected)
        {
            string actual = assignment.ASN01_ReverseString(str);
            Assert.AreEqual(expected, actual);
        }

        [Category("Assignment")]
        [TestCase("radar", true, TestName = "ASN02_StackPalindrome_Radar", Description = "Radar is palindrome")]
        [TestCase("hello", false, TestName = "ASN02_StackPalindrome_Hello", Description = "Hello is not palindrome")]
        [TestCase("a", true, TestName = "ASN02_StackPalindrome_Single", Description = "Single char is palindrome")]
        [TestCase("", true, TestName = "ASN02_StackPalindrome_Empty", Description = "Empty string is palindrome")]
        [TestCase("aba", true, TestName = "ASN02_StackPalindrome_Aba", Description = "Aba is palindrome")]
        [TestCase("abcba", true, TestName = "ASN02_StackPalindrome_Abcba", Description = "Abcba is palindrome")]
        [TestCase("ab", false, TestName = "ASN02_StackPalindrome_Ab", Description = "Ab is not palindrome")]
        [TestCase("aa", true, TestName = "ASN02_StackPalindrome_Aa", Description = "Aa is palindrome")]
        public void Test_ASN02_StackPalindrome(string str, bool expected)
        {
            bool actual = assignment.ASN02_StackPalindrome(str);
            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Extra

        [Category("Extra")]
        [TestCase("()", true, TestName = "EX01_ParenthesesChecker_BalancedSimple", Description = "Simple balanced")]
        [TestCase("([])", true, TestName = "EX01_ParenthesesChecker_BalancedMixed", Description = "Mixed balanced")]
        [TestCase("{[()]}", true, TestName = "EX01_ParenthesesChecker_BalancedNested", Description = "Nested balanced")]
        [TestCase("", true, TestName = "EX01_ParenthesesChecker_Empty", Description = "Empty balanced")]
        [TestCase("(", false, TestName = "EX01_ParenthesesChecker_UnbalancedOpen", Description = "Unbalanced open")]
        [TestCase(")", false, TestName = "EX01_ParenthesesChecker_UnbalancedClose", Description = "Unbalanced close")]
        [TestCase("(]", false, TestName = "EX01_ParenthesesChecker_Mismatch", Description = "Mismatch")]
        [TestCase("([)]", false, TestName = "EX01_ParenthesesChecker_Crossed", Description = "Crossed")]
        [TestCase("((", false, TestName = "EX01_ParenthesesChecker_MultipleOpen", Description = "Multiple open")]
        [TestCase("))", false, TestName = "EX01_ParenthesesChecker_MultipleClose", Description = "Multiple close")]
        public void Test_EX01_ParenthesesChecker(string str, bool expected)
        {
            bool actual = assignment.EX01_ParenthesesChecker(str);
            Assert.AreEqual(expected, actual);
        }

        #endregion
    }

}
