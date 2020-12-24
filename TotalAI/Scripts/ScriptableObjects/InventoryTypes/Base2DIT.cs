using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "Base2DIT", menuName = "Total AI/Inventory Types/Base 2D", order = 0)]
    public class Base2DIT : InventoryType
    {

        public override void ChangeSkin(WorldObject worldObject, GameObject newPrefab, int skinPrefabIndex)
        {
            // -1 means it didn't match any so go back to original
            if (skinPrefabIndex == -1)
            {
                newPrefab = worldObject.PrefabVariant();
            }

            // 2D - just swap out the sprite
            SpriteRenderer spriteRenderer = worldObject.gameObject.GetComponentInChildren<SpriteRenderer>();
            spriteRenderer.sprite = newPrefab.GetComponent<SpriteRenderer>().sprite;

            worldObject.currentSkinPrefabIndex = skinPrefabIndex;
        }

        // Uses 2D Physics
        public override Vector3 FindDropLocation(Entity entity, Entity entityDropped, InventorySlot inventorySlot)
        {
            // First see if a drop location(s) are set on entity
            foreach (Transform child in entity.transform)
            {
                if (child.name == "DropSpot")
                {
                    // TODO: Check to see if this drop spot is occupied
                    return child.transform.position;
                }
            }

            // Pick a random spot on the NavMesh that has no Entities on it already
            Vector3 startLocation = entity.transform.position;
            Vector3 agentForward = entity.transform.forward;

            // Go out far enough to clear the droppedBy Entity
            // TODO: Won't handle multiple renderer GameObjects
            Renderer renderer = entity.GetComponentInChildren<Renderer>();
            float dropDistance = Mathf.Max(renderer.bounds.extents.x, renderer.bounds.extents.y) + inventorySlot.dropDistance;

            Vector3 dropLocation = Vector3.positiveInfinity;
            int attempts = 0;
            while (dropLocation.x == float.PositiveInfinity && attempts< 20)
            {
                Vector3 sampleLocation;
                if (inventorySlot.dropType == InventorySlot.DropType.RandomRadius)
                {
                    Vector2 randomDirection2 = UnityEngine.Random.insideUnitCircle.normalized;
                    Vector3 randomDirection = new Vector3(randomDirection2.x, randomDirection2.y, 0);
                    sampleLocation = startLocation + dropDistance* randomDirection;
                }
                else
                {
                    float offsetDegrees;
                    if (attempts % 2 == 0)
                        offsetDegrees = attempts* 15;
                    else
                        offsetDegrees = attempts;
                    Debug.Log("Start Location = " + startLocation + " - Agent Forward = " + agentForward);
                    Vector3 newDirection = Quaternion.Euler(0, offsetDegrees, 0) * agentForward;
                    Debug.Log("New Direction = " + newDirection);
                    sampleLocation = startLocation + dropDistance * newDirection;
                    Debug.Log("Sample Location = " + sampleLocation);
                }

                // Place it on top of the ground
                Collider2D collider = Physics2D.OverlapCircle(sampleLocation, .1f, LayerMask.GetMask("Ground"));
                if (collider != null)
                {
                    dropLocation = new Vector3(sampleLocation.x, sampleLocation.y, 0);
                }
                else
                {
                    Debug.LogError(entity.name + " is trying to drop inventory (" + name + ") - OverlapCircle did not hit the ground.");
                }
                ++attempts;
            }
            if (dropLocation.x == float.PositiveInfinity)
            {
                Debug.Log(entity.name + " is trying to drop inventory (" + name + ") - 10 attempts couldn't find a drop spot.");
                Vector2 randomDirection2 = UnityEngine.Random.insideUnitCircle.normalized;
                Vector3 randomDirection = new Vector3(randomDirection2.x, 0, randomDirection2.y);
                dropLocation = startLocation + dropDistance* randomDirection;
            }
            return dropLocation;
        }

        public override void Thrown(Entity thrownEntity, Entity thrownByEntity, InventorySlot inventorySlot, Vector3 target, float forceStrength)
        {
            // Unparent and place near Entity
            Entity droppedBy = thrownEntity.inEntityInventory;
            thrownEntity.inEntityInventory = null;

            // TODO: Want to know if Entity cuts out NavMesh
            thrownEntity.transform.parent = null;

            // Reset rotation to prefab rotation - this should be the desired rotation for an Entity when not in inventory
            GameObject prefabVariant = thrownEntity.PrefabVariant();
            if (prefabVariant == null)
                Debug.LogError(name + ": Trying to throw but it is missing prefabVariant index #" + thrownEntity.prefabVariantIndex +
                               " - needed to correctly set rotation of Entity.");
            thrownEntity.transform.rotation = prefabVariant.transform.rotation;

            thrownEntity.gameObject.SetActive(true);

            //transform.position = droppedBy.inventoryType.FindDropLocation(droppedBy, this, inventorySlot);
            thrownEntity.transform.position = thrownByEntity.transform.position;

            // Make sure it has rigidbody and add force
            Rigidbody2D rigidbody2D = thrownEntity.GetComponent<Rigidbody2D>();
            if (rigidbody2D == null)
            {
                Debug.LogError(name + ": Trying to throw Entity but it has no RidgidBody");
                return;
            }
            Vector2 force = ((Vector2)(target - thrownEntity.transform.position)).normalized * forceStrength;
            rigidbody2D.AddForce(force, ForceMode2D.Impulse);
        }
    }
}
