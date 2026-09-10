// ReSharper disable ALL

namespace enet
{
    /// <summary>
    ///     A doubly linked list node holding pointers to the adjacent nodes.
    /// </summary>
    public unsafe struct ENetListNode
    {
        /// <summary>
        ///     Pointer to the next node in the list.
        /// </summary>
        public ENetListNode* next;

        /// <summary>
        ///     Pointer to the previous node in the list.
        /// </summary>
        public ENetListNode* previous;
    }

    /// <summary>
    ///     A doubly linked list anchored by its sentinel node.
    /// </summary>
    public struct ENetList
    {
        /// <summary>
        ///     The sentinel node that anchors the list and terminates iteration.
        /// </summary>
        public ENetListNode sentinel;
    }

    public static unsafe partial class ENet
    {
        /// <summary>
        ///     Returns a pointer to the first node of the list.
        /// </summary>
        /// <param name="list">The list to inspect.</param>
        /// <returns>A pointer to the first node, or the end sentinel when the list is empty.</returns>
        public static ENetListNode* enet_list_begin(ENetList* list) => ((list)->sentinel.next);

        /// <summary>
        ///     Returns a pointer to the end sentinel of the list.
        /// </summary>
        /// <param name="list">The list to inspect.</param>
        /// <returns>A pointer to the sentinel node marking the end of the list.</returns>
        public static ENetListNode* enet_list_end(ENetList* list) => (&(list)->sentinel);

        /// <summary>
        ///     Determines whether the list contains no nodes.
        /// </summary>
        /// <param name="list">The list to inspect.</param>
        /// <returns><see langword="true" /> when the list is empty; otherwise, <see langword="false" />.</returns>
        public static bool enet_list_empty(ENetList* list) => (enet_list_begin(list) == enet_list_end(list));

        /// <summary>
        ///     Returns the node following the given iterator.
        /// </summary>
        /// <param name="iterator">The current node.</param>
        /// <returns>A pointer to the next node in the list.</returns>
        public static ENetListNode* enet_list_next(ENetListNode* iterator) => ((iterator)->next);

        /// <summary>
        ///     Returns the node preceding the given iterator.
        /// </summary>
        /// <param name="iterator">The current node.</param>
        /// <returns>A pointer to the previous node in the list.</returns>
        public static ENetListNode* enet_list_previous(ENetListNode* iterator) => ((iterator)->previous);

        /// <summary>
        ///     Returns the first node of the list as an opaque pointer.
        /// </summary>
        /// <param name="list">The list to inspect.</param>
        /// <returns>A pointer to the first node, or the end sentinel when the list is empty.</returns>
        public static void* enet_list_front(ENetList* list) => ((void*)(list)->sentinel.next);

        /// <summary>
        ///     Returns the last node of the list as an opaque pointer.
        /// </summary>
        /// <param name="list">The list to inspect.</param>
        /// <returns>A pointer to the last node, or the end sentinel when the list is empty.</returns>
        public static void* enet_list_back(ENetList* list) => ((void*)(list)->sentinel.previous);
    }
}