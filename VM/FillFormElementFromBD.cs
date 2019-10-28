using System.Windows.Forms;

namespace VM
{
    static class FillFormElementFromBD
    {
        public static void FillDataGridViewFromBD(ref DataGridView dgv, string request)
        {
            BindingSource bindingSource1 = Requests.Select(request);
            dgv.DataSource = bindingSource1;
        }

        public static void FillComboBoxFromBD(ref ComboBox cb, string request)
        {
            BindingSource bindingSource1 = Requests.Select(request);
            cb.DataSource = bindingSource1;
        }
    }
}
