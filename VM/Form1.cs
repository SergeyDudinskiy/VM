using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

namespace VM
{
    public partial class Form1 : Form
    {       
        int sumOFF = 0; //сумма вложенная клиентом в кофемашину

        public Form1()
        {
            InitializeComponent();
            typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, dataGridView1, new object[] { true }); //чтобы таблица не мерцала при прокрутке 
            typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, dataGridView2, new object[] { true });
            typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, dataGridView3, new object[] { true });
            FillFormElementFromBD.FillDataGridViewFromBD(ref dataGridView1, "SELECT * FROM UWallet");
            FillFormElementFromBD.FillDataGridViewFromBD(ref dataGridView2, "SELECT * FROM Assortment");
            FillFormElementFromBD.FillDataGridViewFromBD(ref dataGridView3, "SELECT * FROM SWallet");
            FillFormElementFromBD.FillComboBoxFromBD(ref comboBox1, "SELECT Nominal FROM UWallet WHERE Count > 0");
            FillFormElementFromBD.FillComboBoxFromBD(ref comboBox2, "SELECT Title FROM Assortment WHERE Count > 0");            

            if (comboBox1.Items.Count > 0 && comboBox2.Items.Count > 0)
            {
                comboBox1.SelectedIndex = comboBox2.SelectedIndex = 0;               
            }
            else
            {
                button1.Enabled = button2.Enabled = button3.Enabled = false;
            }

            if (dataGridView1.ColumnCount > 1)
            {
                dataGridView1.Columns[0].Width = dataGridView1.Columns[1].Width = 70;
            }
            else
            {
                button1.Enabled = button2.Enabled = button3.Enabled = false;
            }         

            if (dataGridView2.ColumnCount > 2)
            {
                dataGridView2.Columns[0].Width = 150;
                dataGridView2.Columns[1].Width = dataGridView2.Columns[2].Width = 60;                 
            }
            else
            {
                button1.Enabled = button2.Enabled = button3.Enabled = false;
            }

            if (dataGridView3.ColumnCount > 1)
            {
                dataGridView3.Columns[0].Width = dataGridView3.Columns[1].Width = 70;
            }
            else
            {
                button1.Enabled = button2.Enabled = button3.Enabled = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int count = Requests.GetValue("SELECT Count FROM UWallet WHERE Nominal = " + comboBox1.Text); //кол-во монет выбранного номинала

            if (count > 0) //если монеты есть
            {
                Requests.Request("UPDATE UWallet SET Count -= 1 WHERE Nominal = " + comboBox1.Text);

                if (Requests.GetValue("SELECT Nominal FROM SWallet WHERE Nominal = " + comboBox1.Text) == -1) //если нашло значение, то оно будет положительное, поэтому сравниваю с -1. Эти 2 условия на случай если в кофемашине нет монет номинала, который есть у пользователя
                {
                    Requests.Request("INSERT INTO SWallet VALUES (" + comboBox1.Text + ", 1)");
                }
                else
                {
                    Requests.Request("UPDATE SWallet SET Count += 1 WHERE Nominal = " + comboBox1.Text);
                }

                bool flag = true;

                try
                {
                    sumOFF += Convert.ToInt32(comboBox1.Text);
                }
                catch
                {
                    flag = false;
                }

                if (flag == true)
                {
                    label2.Text = "Внесённая сумма: " + sumOFF + "р";
                    FillFormElementFromBD.FillDataGridViewFromBD(ref dataGridView1, "SELECT * FROM UWallet");
                    FillFormElementFromBD.FillDataGridViewFromBD(ref dataGridView3, "SELECT * FROM Swallet");
                    FillFormElementFromBD.FillComboBoxFromBD(ref comboBox1, "SELECT Nominal FROM UWallet WHERE Count > 0"); //валидация ввода, т.е пользователь даже не сможет выбрать монеты, которые у него закончились. В таблице я оставил строки номинал монет которых у пользователя был (т.е. у которых на данный момент 0 кол-во)                

                    if (comboBox1.Text == "") //если закончились деньги на кошельке пользователя
                    {
                        button1.Enabled = comboBox1.Enabled = false;
                    }
                }
                else
                {
                    MessageBox.Show("Сбой в работе программы! Программа завершает свою работу!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }
            }
            else
            {
                button1.Enabled = comboBox1.Enabled = false;                 
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
                    sumOFF -= price;
                    label2.Text = "Внесённая сумма: " + sumOFF + "р";
                    Requests.Request("UPDATE Assortment SET Count -= 1 WHERE Title = N'" + comboBox2.Text + "'");                      
                    FillFormElementFromBD.FillDataGridViewFromBD(ref dataGridView2, "SELECT * FROM Assortment");
                    FillFormElementFromBD.FillComboBoxFromBD(ref comboBox2, "SELECT Title FROM Assortment WHERE Count > 0");

                    if (comboBox2.Text == "") //если закончились товары в кофемашине
                    {
                        button2.Enabled = comboBox2.Enabled = false;
                    }

                    MessageBox.Show("Спасибо!", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Недостаточно средств!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }              
            }
            else
            {
                button2.Enabled = comboBox2.Enabled = false;                
            }
        }        

        private void button3_Click(object sender, EventArgs e)
        {
            if (sumOFF != 0)
            {
                int sumVM = 0; //сумма находящаяся в кофемашине (дальше будет подсчитана)
                List<List<int>> list = new List<List<int>>();
                list.Add(Requests.GetList("SELECT Nominal FROM Swallet"));
                list.Add(Requests.GetList("SELECT Count FROM SWallet"));                

                if (list[0].Count != 0 && list[0].Count == list[1].Count)
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
                                    FillFormElementFromBD.FillDataGridViewFromBD(ref dataGridView1, "SELECT * FROM UWallet");
                                    FillFormElementFromBD.FillDataGridViewFromBD(ref dataGridView3, "SELECT * FROM SWallet");
                                    FillFormElementFromBD.FillComboBoxFromBD(ref comboBox1, "SELECT Nominal FROM UWallet WHERE Count > 0"); //валидация ввода, т.е пользователь даже не сможет выбрать монеты, которые у него закончились. В таблице я оставил строки номинал монет которых у пользователя был (т.е. у которых на данный момент 0 кол-во)                
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }

                        button1.Enabled = comboBox1.Enabled = true;                        
                        label2.Text = "Внесённая сумма: " + sumOFF + "р";
                    }
                }
                else
                {
                    MessageBox.Show("В автомате недостаточно сдачи!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }                
            }
            else
            {
                MessageBox.Show("Вы не вносили денег либо внесённая вами сумма полностью израсходавана!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
