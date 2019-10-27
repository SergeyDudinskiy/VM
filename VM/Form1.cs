using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using System.Windows.Forms;

namespace VM
{
    public partial class Form1 : Form
    {
        BindingSource bindingSource1;
        SqlDataAdapter dataAdapter1;    
        int sumOFF = 0; //сумма вложенная клиентом в кофемашину

        public Form1()
        {
            InitializeComponent();
            typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, dataGridView1, new object[] { true }); //чтобы таблица не мерцала при прокрутке 
            typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, dataGridView2, new object[] { true });
            typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, dataGridView3, new object[] { true });
            bindingSource1 = Requests.Select("SELECT * FROM UWallet WHERE Count > 0", ref dataAdapter1);
            dataGridView1.DataSource = bindingSource1;
            bindingSource1 = Requests.Select("SELECT * FROM Assortment WHERE Count > 0", ref dataAdapter1);
            dataGridView2.DataSource = bindingSource1;
            bindingSource1 = Requests.Select("SELECT * FROM SWallet WHERE Count > 0", ref dataAdapter1);
            dataGridView3.DataSource = bindingSource1;
            bindingSource1 = Requests.Select("SELECT Nominal FROM UWallet WHERE Count > 0", ref dataAdapter1);
            comboBox1.DataSource = bindingSource1;
            bindingSource1 = Requests.Select("SELECT Title FROM Assortment WHERE Count > 0", ref dataAdapter1);
            comboBox2.DataSource = bindingSource1;

            if (comboBox1.Items.Count > 0 && comboBox2.Items.Count > 0)
            {
                comboBox1.SelectedIndex = comboBox2.SelectedIndex = 0;               
            }
            else
            {
                button1.Enabled = button2.Enabled = button3.Enabled = false;
            }

            comboBox1.DisplayMember = "Nominal";
            comboBox2.DisplayMember = "Title";

            if (dataGridView1.ColumnCount > 1)
            {
                dataGridView1.Columns[0].Width = dataGridView1.Columns[1].Width = 70;                 
            }

            if (dataGridView2.ColumnCount > 2)
            {
                dataGridView2.Columns[0].Width = 150;
                dataGridView2.Columns[1].Width = dataGridView2.Columns[2].Width = 60;                 
            }

            if (dataGridView3.ColumnCount > 1)
            {
                dataGridView3.Columns[0].Width = dataGridView3.Columns[1].Width = 70;
            }            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int count = Requests.GetValue("SELECT Count FROM UWallet WHERE Nominal = '" + comboBox1.Text + "'"); //кол-во монет выбранного номинала

            if (count > 0) //если монеты есть
            {
                count--;
                Requests.Request("UPDATE UWallet SET Count = " + count + " WHERE Nominal = " + comboBox1.Text);

                if (Requests.GetValue("SELECT Nominal FROM SWallet WHERE Nominal = '" + comboBox1.Text + "'") == -1) //если нашло значение, то оно будет положительное, поэтому сравниваю с -1. эти 2 условия на случай если в кофемашине нету монет номинала, который есть у пользователя
                {
                    Requests.Request("INSERT INTO SWallet VALUES (" + comboBox1.Text + ", 1)");
                }
                else
                {
                    Requests.Request("UPDATE SWallet SET Count += 1 WHERE Nominal = " + comboBox1.Text);
                }

                sumOFF += Convert.ToInt32(comboBox1.Text);
                label2.Text = "Внесённая сумма: " + sumOFF + "р";
                bindingSource1 = Requests.Select("SELECT * FROM UWallet", ref dataAdapter1);
                dataGridView1.DataSource = bindingSource1;
                bindingSource1 = Requests.Select("SELECT * FROM SWallet", ref dataAdapter1);
                dataGridView3.DataSource = bindingSource1;
                bindingSource1 = Requests.Select("SELECT Nominal FROM UWallet WHERE Count > 0", ref dataAdapter1);
                comboBox1.DataSource = bindingSource1;
            }
            else
            {
                button1.Enabled = false;
                comboBox1.Enabled = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int count = Requests.GetValue("SELECT Count FROM Assortment WHERE Title = N'" + comboBox2.Text + "'");
            int price = Requests.GetValue("SELECT Price FROM Assortment WHERE Title = N'" + comboBox2.Text + "'");

            if (count > 0)
            {
                if (sumOFF >= price)
                {
                    count--;
                    sumOFF -= price;
                    Requests.Request("UPDATE Assortment SET Count = " + count + " WHERE Title = N'" + comboBox2.Text + "'");                    
                    label2.Text = "Внесённая сумма: " + sumOFF + "р";
                    bindingSource1 = Requests.Select("SELECT * FROM Assortment", ref dataAdapter1);
                    dataGridView2.DataSource = bindingSource1;
                    bindingSource1 = Requests.Select("SELECT Title FROM Assortment WHERE Count > 0", ref dataAdapter1);
                    comboBox2.DataSource = bindingSource1;
                    MessageBox.Show("Спасибо!", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Недостаточно средств!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }              
            }
            else
            {
                button2.Enabled = false;
                comboBox2.Enabled = false;
            }
        }        

        private void button3_Click(object sender, EventArgs e)
        {
            if (sumOFF != 0)
            {
                int sumVM = 0;
                List<List<int>> list = new List<List<int>>();
                list.Add(Requests.GetList("SELECT Nominal FROM Swallet"));
                list.Add(Requests.GetList("SELECT Count FROM SWallet"));                

                if (list[0].Count != 0)
                {
                    Sort(ref list);

                    for (int i = 0; i < list[0].Count; i++)
                    {
                        sumVM += list[0][i] * list[1][i];                                            
                    }                    

                    if (sumVM < sumOFF)
                    {
                        MessageBox.Show("В автомате недостаточно сдачи!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        for (int i = 0; i < list[0].Count; i++)
                        {
                            int value = list[1][i];                            

                            for (int j = 0; j < value; j++)
                            {
                                if (sumOFF - list[0][i] >= 0)
                                {
                                    sumOFF -= list[0][i];
                                    list[1][0]--;
                                    value--;
                                    Requests.Request("UPDATE UWallet SET Count += 1 WHERE Nominal = " + list[0][i] + "; UPDATE SWallet SET Count -= 1 WHERE Nominal = " + list[0][i]);
                                    bindingSource1 = Requests.Select("SELECT * FROM UWallet", ref dataAdapter1);
                                    dataGridView1.DataSource = bindingSource1;
                                    bindingSource1 = Requests.Select("SELECT * FROM SWallet", ref dataAdapter1);
                                    dataGridView3.DataSource = bindingSource1;
                                }                                
                            }
                        }               

                        label2.Text = "Внесённая сумма: 0р";
                    }
                }
                else
                {
                    MessageBox.Show("В автомате недостаточно сдачи!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }                
            }
            else
            {
                MessageBox.Show("Внесенная вами сумма полностью израсходавана!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Sort(ref List<List<int>> list)
        {
            for (int i = 0; i < list[0].Count - 1; i++)
            {
                for (int j = i + 1; j < list[0].Count; j++)
                {
                    if (list[0][i] < list[0][j])
                    {
                        int temp = list[0][j];
                        list[0][j] = list[0][i];
                        list[0][i] = temp;
                        temp = list[1][j];
                        list[1][j] = list[1][i];
                        list[1][i] = temp;
                    }
                }
            }
        }
    }
}
