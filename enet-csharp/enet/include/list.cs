// ReSharper disable ALL

namespace enet
{
    public unsafe struct ENetListNode
    {
        public ENetListNode* next;
        public ENetListNode* previous;
    }

    public struct ENetList
    {
        public ENetListNode sentinel;
    }

    public static unsafe partial class ENet
    {
        public static ENetListNode* enet_list_begin(ENetList* list) => ((list)->sentinel.next);
        public static ENetListNode* enet_list_end(ENetList* list) => (&(list)->sentinel);

        public static bool enet_list_empty(ENetList* list) => (enet_list_begin(list) == enet_list_end(list));

        public static ENetListNode* enet_list_next(ENetListNode* iterator) => ((iterator)->next);
        public static ENetListNode* enet_list_previous(ENetListNode* iterator) => ((iterator)->previous);

        public static void* enet_list_front(ENetList* list) => ((void*)(list)->sentinel.next);
        public static void* enet_list_back(ENetList* list) => ((void*)(list)->sentinel.previous);
    }
}