// ReSharper disable ALL

namespace enet
{
    public static unsafe partial class ENet
    {
        /// <summary>
        ///     Resets a list to the empty state, linking its sentinel to itself.
        /// </summary>
        /// <param name="list">The list to clear.</param>
        public static void enet_list_clear(ENetList* list)
        {
            list->sentinel.next = &list->sentinel;
            list->sentinel.previous = &list->sentinel;
        }

        /// <summary>
        ///     Inserts a node before the given position in the list.
        /// </summary>
        /// <param name="position">The node before which the new node is inserted.</param>
        /// <param name="data">Pointer to the node to insert.</param>
        /// <returns>A pointer to the inserted node.</returns>
        public static ENetListNode* enet_list_insert(ENetListNode* position, void* data)
        {
            ENetListNode* result = (ENetListNode*)data;

            result->previous = position->previous;
            result->next = position;

            result->previous->next = result;
            position->previous = result;

            return result;
        }

        /// <summary>
        ///     Removes a node from its list, relinking the surrounding nodes.
        /// </summary>
        /// <param name="position">The node to remove.</param>
        /// <returns>A pointer to the removed node.</returns>
        public static void* enet_list_remove(ENetListNode* position)
        {
            position->previous->next = position->next;
            position->next->previous = position->previous;

            return position;
        }

        /// <summary>
        ///     Moves a contiguous range of nodes to before the given position.
        /// </summary>
        /// <param name="position">The node before which the range is moved.</param>
        /// <param name="dataFirst">Pointer to the first node of the range to move.</param>
        /// <param name="dataLast">Pointer to the last node of the range to move.</param>
        /// <returns>A pointer to the first node of the moved range.</returns>
        public static ENetListNode* enet_list_move(ENetListNode* position, void* dataFirst, void* dataLast)
        {
            ENetListNode* first = (ENetListNode*)dataFirst,
                last = (ENetListNode*)dataLast;

            first->previous->next = last->next;
            last->next->previous = first->previous;

            first->previous = position->previous;
            last->next = position;

            first->previous->next = first;
            position->previous = last;

            return first;
        }

        /// <summary>
        ///     Counts the number of nodes in a list.
        /// </summary>
        /// <param name="list">The list to measure.</param>
        /// <returns>The number of nodes in the list.</returns>
        public static nuint enet_list_size(ENetList* list)
        {
            nuint size = 0;
            ENetListNode* position;

            for (position = enet_list_begin(list);
                 position != enet_list_end(list);
                 position = enet_list_next(position))
                ++size;

            return size;
        }
    }
}