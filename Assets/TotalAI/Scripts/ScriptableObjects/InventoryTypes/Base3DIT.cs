using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "Base3DIT", menuName = "Total AI/Inventory Types/Base 3D", order = 0)]
    public class Base3DIT : InventoryType
    {

        public override void ChangeSkin(WorldObject worldObject, GameObject newPrefab, int skinPrefabIndex)
        {
            // -1 means it didn't match any so go back to original
            if (skinPrefabIndex == -1)
            {
                newPrefab = worldObject.PrefabVariant();
            }

            Debug.Log(worldObject.name + ": Base3DIT.ChangeSkin -> " + skinPrefabIndex  + " - " + (newPrefab != null ? newPrefab.name : "Activate"));
            GameObject skinGameObject = null;
            foreach (Transform child in worldObject.transform)
            {
                if (child.name == worldObject.worldObjectType.skinGameObjectName)
                {
                    skinGameObject = child.gameObject;
                }
            }

            if (skinGameObject == null)
            {
                Debug.LogError(name + ": Unable to find a child name that equals \"" + worldObject.worldObjectType.skinGameObjectName +
                                "\" as specificied in its WorldObjectType.skinGameObjectName");
                return;
            }

            WorldObjectType.SkinPrefabMapping skinPrefabMapping = worldObject.worldObjectType.skinPrefabMappings[skinPrefabIndex];
            if (skinPrefabMapping.changeType == WorldObjectType.SkinPrefabMapping.ChangeType.ToggleActive)
            {
                int currentIndex = skinPrefabMapping.childStartIndex + (int)worldObject.completePoints;

                // Find GameObject to activate
                int index = 0;
                if (skinPrefabMapping.enableType == WorldObjectType.SkinPrefabMapping.EnableType.EnableBodyChildren)
                {
                    foreach (Transform child in skinGameObject.transform)
                    {
                        if (index == currentIndex)
                        {
                            child.gameObject.SetActive(true);
                            break;
                        }
                        ++index;
                    }
                }
                else
                {
                    bool foundGameObject = false;
                    foreach (Transform parent in skinGameObject.transform)
                    {
                        foreach (Transform child in parent.transform)
                        {
                            if (index == currentIndex)
                            {
                                child.gameObject.SetActive(true);
                                foundGameObject = true;
                                break;
                            }
                            ++index;
                        }
                        if (foundGameObject)
                            break;
                    }
                }

                // Figure out what needs to be deactivated
                if (skinPrefabMapping.disableType == WorldObjectType.SkinPrefabMapping.DisableType.First)
                {
                    skinGameObject.transform.GetChild(0).gameObject.SetActive(false);
                }
                else if (skinPrefabMapping.disableType == WorldObjectType.SkinPrefabMapping.DisableType.Previous)
                {
                    skinGameObject.transform.GetChild(currentIndex - 1).gameObject.SetActive(false);
                }
            }
            else
            {
                Destroy(skinGameObject);
             
                GameObject newGameObject = Instantiate(newPrefab, worldObject.transform);
                newGameObject.name = worldObject.worldObjectType.skinGameObjectName;
                newGameObject.layer = worldObject.gameObject.layer;

                Debug.Log("Changed the prefab for " + worldObject.name + ": Prefab Name = " + newPrefab.name);
            }

            // Check to see if transform info should change if its in inventory
            if (worldObject.inEntityInventory != null)
            {
                Entity inventoryOwner = worldObject.inEntityInventory;
                InventorySlot inventorySlot = inventoryOwner.inventoryType.GetEntitySlot(inventoryOwner, worldObject);
                GameObject gameObject = worldObject.worldObjectType.GetTransformMapping(inventorySlot, inventoryOwner, worldObject);
                worldObject.transform.localPosition = gameObject.transform.position;
                worldObject.transform.localRotation = gameObject.transform.rotation;
                worldObject.transform.localScale = gameObject.transform.localScale;
            }

            worldObject.totalAIManager.UpdateAllNavMeshes();
            worldObject.currentSkinPrefabIndex = skinPrefabIndex;
        }

        // Uses 3D Physics
        public override Vector3 FindDropLocation(Entity entity, Entity entityDropped, InventorySlot inventorySlot)
        {
            // Pick a random spot on the NavMesh that has no Entities on it already
            Vector3 startLocation = entity.transform.position;
            Vector3 agentForward = entity.transform.forward;

            // Go out far enough to clear the droppedBy Entity
            // TODO: Won't handle multiple renderer GameObjects
            Renderer renderer = entity.GetComponentInChildren<Renderer>();
            float dropDistance = Mathf.Max(renderer.bounds.extents.x, renderer.bounds.extents.z) + inventorySlot.dropDistance;

            Vector3 dropLocation = Vector3.positiveInfinity;
            int attempts = 0;
            while (dropLocation.x == float.PositiveInfinity && attempts< 20)
            {
                Vector3 sampleLocation;
                if (inventorySlot.dropType == InventorySlot.DropType.RandomRadius)
                {
                    Vector2 randomDirection2 = UnityEngine.Random.insideUnitCircle.normalized;
                    Vector3 randomDirection = new Vector3(randomDirection2.x, 0, randomDirection2.y);
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
                if (Physics.BoxCast(sampleLocation + Vector3.up * 30, new Vector3(.5f, .5f, .5f), Vector3.down, out RaycastHit hit, Quaternion.identity, 35f,
                                    LayerMask.GetMask("Ground", "Agent", "WorldObject"), QueryTriggerInteraction.Ignore))
                {
                    Debug.Log(Mathf.Abs(hit.point.y - sampleLocation.y));
                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground") && Mathf.Abs(hit.point.y - sampleLocation.y) < 2f)
                    {
                        dropLocation = new Vector3(sampleLocation.x, hit.point.y, sampleLocation.z);
                    }
                }
                else
                {
                    Debug.LogError(entity.name + " is trying to drop inventory (" + name + ") - raycast down did not hit the ground.");
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
            
            thrownEntity.transform.parent = null;

            // Reset rotation to prefab rotation - this should be the desired rotation for an Entity when not in inventory
            GameObject prefabVariant = thrownEntity.PrefabVariant();
            if (prefabVariant == null)
                Debug.LogError(name + ": Trying to throw but it is missing prefabVariant index #" + thrownEntity.prefabVariantIndex +
                               " - needed to correctly set rotation of Entity.");
            thrownEntity.transform.rotation = prefabVariant.transform.rotation;

            thrownEntity.gameObject.SetActive(true);

            //transform.position = droppedBy.inventoryType.FindDropLocation(droppedBy, this, inventorySlot);
            //thrownEntity.transform.position = thrownByEntity.transform.position;

            // Make sure it has rigidbody and add force
            Rigidbody rigidbody = thrownEntity.GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                Debug.LogError(name + ": Trying to throw Entity but it has no RidgidBody");
                return;
            }
            //thrownEntity.transform.LookAt(target);
            Vector3 thrownEntityPosition = new Vector3(thrownEntity.transform.position.x, 0, thrownEntity.transform.position.z);
            Vector3 force = (target - thrownEntityPosition).normalized * forceStrength;
            rigidbody.AddForce(force, ForceMode.Impulse);
        }
    }
}
