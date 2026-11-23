using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Solution
{

    public class OOPPlayer : Character
    {
        public Inventory inventory;
        public ActionHistoryManager actionHistoryManager;
        private InputAction moveAction;
        private InputAction fireAction;

        public bool isAutoMoving = false; // Flag to control auto-movement

        public override void SetUP()
        {
            base.SetUP();
            PrintInfo();
            GetRemainEnergy();
            inventory = GetComponent<Inventory>();
            moveAction = InputSystem.actions.FindAction("Move");
            fireAction = InputSystem.actions.FindAction("Attack");
            actionHistoryManager.SaveStateForUndo(new Vector2(positionX, positionY));
        }

        public void Update()
        {
            // Manual input is only processed if not in auto-move mode
            if (isAutoMoving)
            {
                return;
            }

            if (moveAction != null && moveAction.triggered)
            {
                Move(moveAction.ReadValue<Vector2>());
            }

            Keyboard keyboard = Keyboard.current;
            bool firePressed = fireAction != null && fireAction.triggered;
            if (keyboard != null)
            {
                firePressed |= keyboard.eKey.wasPressedThisFrame;
            }

            if (firePressed)
            {
                UseFireStorm();
            }

            if (keyboard == null)
            {
                return;
            }

            if (keyboard.qKey.wasPressedThisFrame)
            {
                actionHistoryManager.StartAutoMoveSequence(this);
            }
        }
        public override void Move(Vector2 direction)
        {
            HistoryManager.Instance.BeginTurn();
            MoveCore(direction);
            HistoryManager.Instance.EndTurn();
        }
        public void MoveCore(Vector2 direction)
        {
            base.Move(direction);
            mapGenerator.MoveEnemies();
            // Save the state AFTER the move
            actionHistoryManager.SaveStateForUndo(new Vector2(positionX, positionY));
      
        }

        public void UseFireStorm()
        {
            if (inventory.HasItem("FireStorm",1))
            {
                inventory.UseItem("FireStorm",1);
                OOPEnemy[] enemies = UtilitySortEnemies.SortEnemiesByRemainningEnergy1(mapGenerator);
                int count = 3;
                if (count > enemies.Length)
                {
                    count = enemies.Length;
                }
                for (int i = 0; i < count; i++)
                {
                    enemies[i].TakeDamage(10);
                }
            }
            else
            {
                //Debug.Log("No FireStorm in inventory");
            }
        }
        
        public void Attack(OOPEnemy _enemy)
        {
            _enemy.TakeDamage(AttackPoint);
            Debug.Log(_enemy.name + " is energy " + _enemy.energy);
        }
        protected override void CheckDead()
        {
            base.CheckDead();
            if (energy <= 0)
            {
                Debug.Log("Player is Dead");
            }
        }

    }

}
