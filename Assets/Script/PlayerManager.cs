using System.Collections;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

namespace UnityEngine
{
    public class PlayerManager : MonoBehaviour
    {
        [Header("Player Stats")]
        public int playerHealth = 100;
        public int currentHealth;
        public int lifesteal = 8;
        public int defense = 5;

        public int money = 0;
        public int damage = 20;
        public int slotMax = 2;

        [Header("Respawn Settings")]
        private Vector3 respawnPoint;
        private Quaternion respawnRotation;

        private Animator anim;
        private bool canTakeDamage = true;

        public TPCharacter thirdPersonCharacter;

        private void Start()
        {
            anim = GetComponent<Animator>();

            if (SaveManager.SaveExists())
            {
                SaveData data = SaveManager.LoadGame();
                transform.position = data.checkpointPosition;
                currentHealth = data.playerHealth;
                money = data.playerMoney;

                respawnPoint = data.checkpointPosition;
                respawnRotation = transform.rotation;

                Debug.Log("Player loaded from save file.");
            }
            else
            {
                Debug.Log("No save found, starting fresh.");
                currentHealth = playerHealth;
                respawnPoint = transform.position;
                respawnRotation = transform.rotation;
            }
        }

        public void TakeDamage(int amount)
        {
            if (!canTakeDamage) return;

            // Apply defense
            int finalDamage = Mathf.Max(0, amount - defense);
            currentHealth -= finalDamage;

            Debug.Log($"Player took {finalDamage} damage (blocked {amount - finalDamage}). Current HP: {currentHealth}");

            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                if (anim != null)
                    anim.SetTrigger("Hurt");
            }
        }

        /// <summary>
        /// Heal player for lifesteal when dealing damage to an enemy.
        /// </summary>
        public void ApplyLifesteal()
        {
            if (lifesteal <= 0) return;

            currentHealth += lifesteal;
            if (currentHealth > playerHealth)
                currentHealth = playerHealth;

            Debug.Log($"Lifesteal +{lifesteal} HP. Current HP: {currentHealth}");
        }

        private void Die()
        {
            Debug.Log("Player has died!");

            if (anim != null)
                anim.SetTrigger("Die");

            StartCoroutine(RespawnAfterDelay(2f));
        }

        private IEnumerator RespawnAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            Respawn();
        }

        public void Respawn()
        {
            CharacterController controller = GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;

            transform.position = respawnPoint;
            transform.rotation = respawnRotation;

            if (controller != null) controller.enabled = true;

            currentHealth = playerHealth;

            EnemyRespawner respawner = FindObjectOfType<EnemyRespawner>();
            if (respawner != null)
                respawner.RespawnEnemy();

            Debug.Log("Player respawned at last checkpoint!");
        }

        public void SetCheckpoint(Vector3 position, Quaternion rotation)
        {
            respawnPoint = position;
            respawnRotation = rotation;
            Debug.Log($"Checkpoint set at: {position}");
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
            // when dealing damage to an enemy, apply lifesteal here or in the enemy’s damage receiver
            ApplyLifesteal();
            return damage;
        }

        public void SetInvincible()
        {
            canTakeDamage = false;
            Debug.Log("Player is now INVINCIBLE");
        }

        public void SetVulnerable()
        {
            canTakeDamage = true;
            Debug.Log("Player is now VULNERABLE");
        }
    }
}
