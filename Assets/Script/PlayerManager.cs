using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

namespace UnityEngine
{
    public class PlayerManager : MonoBehaviour
    {
        [Header("Player Stats")]
        public int playerHealth = 100;
        public int currentHealth;

        public int money = 0;
        public int damage = 20;
        public int slotMax = 2;

        private Animator anim;
        private bool canTakeDamage = true;

        public TPCharacter thirdPersonCharacter;
        

        void Start()
        {
            currentHealth = playerHealth;
            anim = GetComponent<Animator>();
        }

        //public void Knockback(Vector3 knockbackDir, float force, float upward)
        //{
        //    Rigidbody rb = GetComponent<Rigidbody>();
        //    rb.AddForce(knockbackDir * force + Vector3.up * upward, ForceMode.Impulse);
        //}
        public void TakeDamage(int amount)
        {


            currentHealth -= amount;
            Debug.Log("current health" + currentHealth);

            if (!canTakeDamage) return;

            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                // Optional: play hurt animation
                if (anim != null)
                    anim.SetTrigger("Hurt");
            }
        }

        private void Die()
        {
            // Optional: death animation
            if (anim != null)
                anim.SetTrigger("Die");

            Debug.Log("Player has died!");

            // You could reload scene or disable controls here
            // Example:
            // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }


        public void AddMoney(int amount)
        {
            money += amount;
            Debug.Log("Money increased! Current money: " + money);
        }

        public void SpendMoney(int amount)
        {
            if (money >= amount)
            {
                money -= amount;
                Debug.Log("Money spent! Current money: " + money);
            }
            else
            {
                Debug.Log("Not enough money!");
            }
        }

        public int DealDamage()
        {
            return damage;
        }

        public void SetInvincible()
        {
            canTakeDamage = false;
            Debug.Log("Player is now INVINCIBLE");
        }

        //  Turn damage back on
        public void SetVulnerable()
        {
            canTakeDamage = true;
            Debug.Log("Player is now VULNERABLE");
        }
    }

}