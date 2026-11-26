using System;
using System.Collections.Generic;

namespace Assignment
{
    public class StudentSolution
    {
        class Action
        {
            public string Name;
            public int Value;
        }

        #region Lecture

        public (int Count, int Popped, int Peek, int CountAfterPeek) LCT01_StackSyntax()
        {
            Stack<int> stack = new Stack<int>();
            stack.Push(1);
            stack.Push(2);
            stack.Push(3); // top

            int count = stack.Count; // 3
            int popped = stack.Pop(); // popped = 3
            int peek = stack.Peek(); // peek = 2
            int countAfterPeek = stack.Count; // countAfterPeek = 2

            return (count, popped, peek, countAfterPeek);
        }

        public (int Count, int Dequeued, int Peek, int CountAfterPeek) LCT02_QueueSyntax()
        {
            Queue<int> queue = new Queue<int>();
            queue.Enqueue(1); // head
            queue.Enqueue(2);
            queue.Enqueue(3);

            int count = queue.Count; // 3
            int dequeued = queue.Dequeue(); // dequeued = 1
            int peek = queue.Peek(); // peek = 2
            int countAfterPeek = queue.Count; // countAfterPeek = 2

            return (count, dequeued, peek, countAfterPeek);
        }

        public string[] LCT03_ActionStack()
        {
            Action action1 = new Action { Name = "Action 1" };
            Action action2 = new Action { Name = "Action 2" };
            Action action3 = new Action { Name = "Action 3" };

            Stack<Action> stack = new Stack<Action>();
            stack.Push(action1);
            stack.Push(action2);
            stack.Push(action3);

            int c = stack.Count;
            List<string> actions = new List<string>();
            for (int i = 0; i < c; i++)
            {
                var action = stack.Pop();
                actions.Add(action.Name);
            }

            // actions => action3, action2, action1
            return actions.ToArray();
        }


        public string[] LCT04_ActionQueue()
        {
            Action action1 = new Action { Name = "Action 1" };
            Action action2 = new Action { Name = "Action 2" };
            Action action3 = new Action { Name = "Action 3" };

            Queue<Action> queue = new();
            queue.Enqueue(action1);
            queue.Enqueue(action2);
            queue.Enqueue(action3);

            int c = queue.Count;
            List<string> actions = new List<string>();
            for (int i = 0; i < c; i++)
            {
                //Action a = queue.Dequeue();
                //actions.Add(queue.Dequeue().Name);
                var a = queue.Dequeue();
                actions.Add(a.Name);
            }

            return actions.ToArray();
        }

        #endregion

        #region Assignment

        public string ASN01_ReverseString(string str) // superman
        {
            // return namrepus
            throw new NotImplementedException();
        }

        public bool ASN02_StackPalindrome(string str) // sus, dad, aannaa
        {
            // retunr true if str is palindrome
            throw new NotImplementedException();
        }

        #endregion

        #region Extra

        public bool EX01_ParenthesesChecker(string str) // str = ((() [{(}])
        {
            // ((() return false
            throw new NotImplementedException();
        }

        #endregion
    }
}
