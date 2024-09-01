// ReSharper disable ALL

namespace enet
{
    public static unsafe partial class ENet
    {
        public static void enet_list_clear(ENetList* list)
        {
            list->sentinel.next = &list->sentinel;
            list->sentinel.previous = &list->sentinel;
        }

        public static ENetListNode* enet_list_insert(ENetListNode* position, void* data)
        {
            ENetListNode* result = (ENetListNode*)data;

            result->previous = position->previous;
            result->next = position;

            result->previous->next = result;
            position->previous = result;

            return result;
        }

        public static void* enet_list_remove(ENetListNode* position)
        {
            position->previous->next = position->next;
            position->next->previous = position->previous;

            return position;
        }

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

        public static nint enet_list_size(ENetList* list)
        {
            nint size = 0;
            ENetListNode* position;

            for (position = enet_list_begin(list);
                 position != enet_list_end(list);
                 position = enet_list_next(position))
                ++size;

            return size;
        }
    }
}