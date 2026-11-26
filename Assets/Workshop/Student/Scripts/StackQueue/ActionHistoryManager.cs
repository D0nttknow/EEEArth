using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
namespace Solution
{
    public class ActionHistoryManager : MonoBehaviour
    {
        // 1. Undo System using Stack
        private Stack<Vector2> undoStack = new Stack<Vector2>();
        private Stack<Vector2> redoStack = new Stack<Vector2>();
        // 2. Auto-Move System using Queue
        private Queue<Vector2> autoMoveQueue = new Queue<Vector2>();

        #region "This Is undoStack Function"

        /// Saves the current player state (position) to the undo stack.
        public void SaveStateForUndo(Vector2 currentPosition)
        {
            // Only push a state if it's different from the last saved state 
            // (optional optimization, but good practice for movement)
            undoStack.Push(currentPosition);
            redoStack.Clear();
        
        }
        /// Reverts the player's state to the previous one using the undo stack.
        /// </summary>
        public void UndoLastMove(OOPPlayer player)
        {
            if (undoStack.Count == 0)
            {
                return;
            }
            // Need at least two states: the current one, and the one to revert to.

            var p = undoStack.Pop();
            redoStack.Push(p);
            //redoStack.Push(undoStack.Pop());

            var prevPosition = undoStack.Peek();
            player.UpdatePosition((int)prevPosition.x, (int)prevPosition.y);
        }
        public void RedoLastMove(OOPPlayer player)
        {
            if (redoStack.Count == 0)
            {
                return;
            }
            // Need at least two states: the current one, and the one to revert to.
            var p = redoStack.Pop();
            undoStack.Push(p);
            player.UpdatePosition((int)p.x, (int)p.y);
        }
        #endregion

        #region "This Is autoMoveQueue Function"

        public void StartAutoMoveSequence(OOPPlayer player)
        {
            //create a sample sequence of moves
            // 1. prepare the Queue with the sequence of moves
            Vector2[] moves = new Vector2[] {
                Vector2.right,
                Vector2.right,
                Vector2.up,
                Vector2.left,
                Vector2.up,
                Vector2.down,
            };

            for (int i = 0; i < moves.Length; i++)
            {
                autoMoveQueue.Enqueue(moves[i]);
            }

            // Start the coroutine to process the auto-move sequence
            StartCoroutine(ProcessAutoMoveSequence(player));
        }
        public IEnumerator ProcessAutoMoveSequence(OOPPlayer player)
        {
            player.isAutoMoving = true;
            Debug.Log($"Auto-move sequence started with {autoMoveQueue.Count} steps.");

            // 2. process the Queue step-by-step
            yield return new WaitForSeconds(0.5f); // Initial delay before starting

            player.isAutoMoving = false;
            Debug.Log("Auto-move sequence finished.");
        }

        #endregion

    }
}

