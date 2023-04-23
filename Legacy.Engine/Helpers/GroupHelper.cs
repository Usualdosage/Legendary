// <copyright file="GroupHelper.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Helpers
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Helper class for managing player groups.
    /// </summary>
    public class GroupHelper
    {
        /// <summary>
        /// Resets all groups.
        /// </summary>
        public static void ResetGroups()
        {
            Communicator.Groups = new System.Collections.Concurrent.ConcurrentDictionary<long, List<long>>();
        }

        /// <summary>
        /// Remove a group by ID.
        /// </summary>
        /// <param name="groupId">The group ID.</param>
        /// <returns>True if removed.</returns>
        public static bool RemoveGroup(long groupId)
        {
            return Communicator.Groups.TryRemove(groupId, out List<long>? dummy);
        }

        /// <summary>
        /// Gets a value indicating if the given character ID is part of a group.
        /// </summary>
        /// <param name="characterId">The character id.</param>
        /// <returns>True if in group.</returns>
        public static bool IsInGroup(long characterId)
        {
            var result = Communicator.Groups.Any(g => g.Value.Contains(characterId));

            // Could be the owner.
            if (!result)
            {
                return IsGroupOwner(characterId);
            }
            else
            {
                return result;
            }
        }

        /// <summary>
        /// Gets a value indicating if the actor and the character are in the same group.
        /// </summary>
        /// <param name="character1">The first character.</param>
        /// <param name="character2">The second character.</param>
        /// <returns>True if the character IDs are in the same group.</returns>
        public static bool IsGroupedWith(long character1, long character2)
        {
            var result = Communicator.Groups.Any(g => g.Value.Contains(character1) && g.Value.Contains(character2));

            if (!result)
            {
                // One of them could be the owner.
                if (IsGroupOwner(character1))
                {
                    var group = GetGroup(character1);
                    if (group != null)
                    {
                        return group.Value.Value.Any(g => g == character2);
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (IsGroupOwner(character2))
                {
                    var group = GetGroup(character2);
                    if (group != null)
                    {
                        return group.Value.Value.Any(g => g == character1);
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating if the character is the owner of a group.
        /// </summary>
        /// <param name="characterId">The character id.</param>
        /// <returns>True if the character is the group owner.</returns>
        public static bool IsGroupOwner(long characterId)
        {
            return Communicator.Groups.Any(g => g.Key == characterId);
        }

        /// <summary>
        /// Adds a character to a group, if it exists. If not, creates the group.
        /// </summary>
        /// <param name="groupId">The group id (id of the character who is the group leader).</param>
        /// <param name="characterId">The character id to add.</param>
        /// <returns>True if added.</returns>
        public static bool AddToGroup(long groupId, long characterId)
        {
            if (Communicator.Groups.Any(g => g.Key == groupId))
            {
                if (!Communicator.Groups[groupId].Contains(characterId))
                {
                    Communicator.Groups[groupId].Add(characterId);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return Communicator.Groups.TryAdd(groupId, new List<long> { characterId });
            }
        }

        /// <summary>
        /// Removes a character from a group, if it exists.
        /// </summary>
        /// <param name="groupId">The group id (id of the character who is the group leader).</param>
        /// <param name="characterId">The character id to remove.</param>
        /// <returns>True if removed.</returns>
        public static bool RemoveFromGroup(long groupId, long characterId)
        {
            if (Communicator.Groups.Any(g => g.Key == groupId))
            {
                Communicator.Groups[groupId].Remove(characterId);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Removes a list of characters from a group.
        /// </summary>
        /// <param name="groupId">The group id.</param>
        /// <param name="characterIds">The list of character IDs to remove.</param>
        /// <returns>True if removed.</returns>
        public static bool RemoveFromGroup(long groupId, List<long> characterIds)
        {
            if (Communicator.Groups.Any(g => g.Key == groupId))
            {
                Communicator.Groups[groupId].RemoveAll(g => characterIds.Contains(g));
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Removes all members of a group.
        /// </summary>
        /// <param name="groupId">The group ID.</param>
        /// <returns>True if successful.</returns>
        public static bool EmptyGroup(long groupId)
        {
            if (Communicator.Groups.Any(g => g.Key == groupId))
            {
                Communicator.Groups[groupId].RemoveRange(0, Communicator.Groups[groupId].Count - 1);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Removes a character from any and all groups, including their own. They should only ever be a member of one.
        /// </summary>
        /// <param name="characterId">The character id.</param>
        public static void RemoveFromAllGroups(long characterId)
        {
            if (IsInGroup(characterId))
            {
                var allGroups = Communicator.Groups.Where(g => g.Value.Contains(characterId));

                foreach (var kvp in allGroups)
                {
                    Communicator.Groups[kvp.Key].Remove(characterId);
                }
            }
        }

        /// <summary>
        /// Get the group ID of the group the player is currently in.
        /// </summary>
        /// <param name="characterId">The character id.</param>
        /// <returns>The group ID.</returns>
        public static long? GetGroupId(long characterId)
        {
            if (IsInGroup(characterId))
            {
                var group = Communicator.Groups.Single(g => g.Value.Contains(characterId));
                return group.Key;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the group that a character is a member of, regardless of whether they are the owner or not.
        /// </summary>
        /// <param name="characterId">The character id.</param>
        /// <returns>Group or null.</returns>
        public static KeyValuePair<long, List<long>>? FindParentGroup(long characterId)
        {
            var defaultGroup = default(KeyValuePair<long, List<long>>);
            var group = Communicator.Groups.FirstOrDefault(g => g.Key == characterId);

            if (group.Equals(defaultGroup))
            {
                group = Communicator.Groups.FirstOrDefault(g => g.Value.Contains(characterId));
                if (group.Equals(defaultGroup))
                {
                    return null;
                }
                else
                {
                    return group;
                }
            }
            else
            {
                return group;
            }
        }

        /// <summary>
        /// Gets the group by group ID.
        /// </summary>
        /// <param name="groupId">The group ID.</param>
        /// <returns>Group.</returns>
        public static KeyValuePair<long, List<long>>? GetGroup(long groupId)
        {
            var defaultGroup = default(KeyValuePair<long, List<long>>);
            var group = Communicator.Groups.FirstOrDefault(g => g.Key == groupId);

            if (group.Equals(defaultGroup))
            {
                return null;
            }
            else
            {
                return group;
            }
        }

        /// <summary>
        /// Gets all group members, including the owner/leader, for a given group.
        /// </summary>
        /// <param name="characterId">The character ID.</param>
        /// <returns>List of character IDs in the group.</returns>
        public static List<long>? GetAllGroupMembers(long characterId)
        {
            if (Communicator.Groups.TryGetValue(characterId, out List<long>? groupMembers))
            {
                if (groupMembers != null)
                {
                    // Add the group ID, as this is the group leader.
                    if (!groupMembers.Contains(characterId))
                    {
                        groupMembers.Add(characterId);
                    }

                    return groupMembers;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                // Was not found by group ID, so get group(s) that have a value containing the groupId.
                var defaultGroup = default(KeyValuePair<long, List<long>>);
                var group = Communicator.Groups.FirstOrDefault(g => g.Value.Contains(characterId));

                if (group.Equals(defaultGroup))
                {
                    return null;
                }
                else
                {
                    // Get all members, including the owner.
                    List<long> allMembers = new ();
                    allMembers.AddRange(group.Value);
                    allMembers.Add(group.Key);
                    return allMembers;
                }
            }
        }
    }
}