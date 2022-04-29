// Auto-generated by geninterop.py.
// DO NOT MODIFIY BY HAND.


#if PYTHON27
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Text;

namespace pyRevitLabs.PythonNet
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal class TypeOffset
    {
        static TypeOffset()
        {
            Type type = typeof(TypeOffset);
            FieldInfo[] fi = type.GetFields();
            int size = IntPtr.Size;
            for (int i = 0; i < fi.Length; i++)
            {
                fi[i].SetValue(null, i * size);
            }
        }

        public static int magic()
        {
            return ob_size;
        }

        // Auto-generated from PyHeapTypeObject in Python.h
        public static int ob_refcnt = 0;
        public static int ob_type = 0;
        public static int ob_size = 0;
        public static int tp_name = 0;
        public static int tp_basicsize = 0;
        public static int tp_itemsize = 0;
        public static int tp_dealloc = 0;
        public static int tp_print = 0;
        public static int tp_getattr = 0;
        public static int tp_setattr = 0;
        public static int tp_compare = 0;
        public static int tp_repr = 0;
        public static int tp_as_number = 0;
        public static int tp_as_sequence = 0;
        public static int tp_as_mapping = 0;
        public static int tp_hash = 0;
        public static int tp_call = 0;
        public static int tp_str = 0;
        public static int tp_getattro = 0;
        public static int tp_setattro = 0;
        public static int tp_as_buffer = 0;
        public static int tp_flags = 0;
        public static int tp_doc = 0;
        public static int tp_traverse = 0;
        public static int tp_clear = 0;
        public static int tp_richcompare = 0;
        public static int tp_weaklistoffset = 0;
        public static int tp_iter = 0;
        public static int tp_iternext = 0;
        public static int tp_methods = 0;
        public static int tp_members = 0;
        public static int tp_getset = 0;
        public static int tp_base = 0;
        public static int tp_dict = 0;
        public static int tp_descr_get = 0;
        public static int tp_descr_set = 0;
        public static int tp_dictoffset = 0;
        public static int tp_init = 0;
        public static int tp_alloc = 0;
        public static int tp_new = 0;
        public static int tp_free = 0;
        public static int tp_is_gc = 0;
        public static int tp_bases = 0;
        public static int tp_mro = 0;
        public static int tp_cache = 0;
        public static int tp_subclasses = 0;
        public static int tp_weaklist = 0;
        public static int tp_del = 0;
        public static int tp_version_tag = 0;
        public static int nb_add = 0;
        public static int nb_subtract = 0;
        public static int nb_multiply = 0;
        public static int nb_divide = 0;
        public static int nb_remainder = 0;
        public static int nb_divmod = 0;
        public static int nb_power = 0;
        public static int nb_negative = 0;
        public static int nb_positive = 0;
        public static int nb_absolute = 0;
        public static int nb_nonzero = 0;
        public static int nb_invert = 0;
        public static int nb_lshift = 0;
        public static int nb_rshift = 0;
        public static int nb_and = 0;
        public static int nb_xor = 0;
        public static int nb_or = 0;
        public static int nb_coerce = 0;
        public static int nb_int = 0;
        public static int nb_long = 0;
        public static int nb_float = 0;
        public static int nb_oct = 0;
        public static int nb_hex = 0;
        public static int nb_inplace_add = 0;
        public static int nb_inplace_subtract = 0;
        public static int nb_inplace_multiply = 0;
        public static int nb_inplace_divide = 0;
        public static int nb_inplace_remainder = 0;
        public static int nb_inplace_power = 0;
        public static int nb_inplace_lshift = 0;
        public static int nb_inplace_rshift = 0;
        public static int nb_inplace_and = 0;
        public static int nb_inplace_xor = 0;
        public static int nb_inplace_or = 0;
        public static int nb_floor_divide = 0;
        public static int nb_true_divide = 0;
        public static int nb_inplace_floor_divide = 0;
        public static int nb_inplace_true_divide = 0;
        public static int nb_index = 0;
        public static int mp_length = 0;
        public static int mp_subscript = 0;
        public static int mp_ass_subscript = 0;
        public static int sq_length = 0;
        public static int sq_concat = 0;
        public static int sq_repeat = 0;
        public static int sq_item = 0;
        public static int sq_slice = 0;
        public static int sq_ass_item = 0;
        public static int sq_ass_slice = 0;
        public static int sq_contains = 0;
        public static int sq_inplace_concat = 0;
        public static int sq_inplace_repeat = 0;
        public static int bf_getreadbuffer = 0;
        public static int bf_getwritebuffer = 0;
        public static int bf_getsegcount = 0;
        public static int bf_getcharbuffer = 0;
        public static int bf_getbuffer = 0;
        public static int bf_releasebuffer = 0;
        public static int name = 0;
        public static int ht_slots = 0;

        /* here are optional user slots, followed by the members. */
        public static int members = 0;
    }
}

#endif
