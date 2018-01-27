using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DataSetHW
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string constStr = "Data Source = DESKTOP-DKRR6L1; Initial Catalog=DataSet; User Id = Natalya; Password = 123;";
        public DataSet ds = new DataSet("DataSet");
        public SqlConnection con;
        public MainWindow()
        {
            InitializeComponent();
            con = new SqlConnection(constStr);
            DataTable dt = new DataTable("TrackEvalutionPart");
            DataTable dt2 = new DataTable("TrackComponent");
            DataTable dt3 = new DataTable("PMChecklistPart");
            ds.Tables.Add(dt);
            ds.Tables.Add(dt2);
            ds.Tables.Add(dt3);

            foreach (DataTable item in ds.Tables)
            {
                CreateTable(item,con);
                ComboBoxItem cbi = new ComboBoxItem();
                cbi.Content = item.TableName;
                TablesComboBox.Items.Add(cbi);
            }
          
        }

        private List<MyT> MethodRow(DataTable dt)
        {
                List<MyT> someData = new List<MyT>();
            
          foreach (DataRow row in dt.Rows)
            {
                MyT m = new MyT();

                for(int i = 0; i<row.ItemArray.Count();i++)
                {
                    m.value += string.Format(dt.Columns[i].ColumnName + " = " + row.ItemArray[i] + " ");
                }
                        
                someData.Add(m);
            }

            return someData;
         
        }

        public class MyT
        {
            public string value { get; set; }
        }

        public static void CreateTable(DataTable dt, SqlConnection con)
        {
            try
            {
                con.Open();
                int i = 0; float f = 0; string s = "";

                SqlCommand cmd = new SqlCommand("exec sp_columns " + dt.TableName, con);

                SqlDataReader dr = cmd.ExecuteReader();

                DataColumn prim = new DataColumn();

                while (dr.Read())
                {
                    if (dr[5].ToString().Contains("int") && !dr[5].ToString().Contains("identity") || dr[5].ToString().Contains("bit"))
                    {
                        dt.Columns.Add(new DataColumn(dr[3].ToString(), i.GetType()));
                    }

                    if (dr[5].ToString().Contains("float"))
                    {
                        dt.Columns.Add(new DataColumn(dr[3].ToString(), f.GetType()));
                    }

                    if (dr[5].ToString().Contains("varchar") || dr[5].ToString().Contains("date"))
                    {
                        dt.Columns.Add(new DataColumn(dr[3].ToString(), s.GetType()));
                    }

                    if (dr[5].ToString().Contains("identity"))
                    {
                        prim = new DataColumn(dr[3].ToString(), i.GetType());
                        dt.Columns.Add(prim);
                    }

                }
                dt.PrimaryKey = new DataColumn[]
              {
                  prim
              };

                prim.AllowDBNull = false;
                prim.AutoIncrement = true;
                prim.AutoIncrementSeed = 0;
                prim.AutoIncrementStep = 1;

                con.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void AllTablesDataButton_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<MyT> all = new List<MyT>();

            foreach (DataTable item in ds.Tables)
            {
                try
                {


                    con.Open();

                    SqlDataAdapter da = new SqlDataAdapter("Select * from " + item.TableName, con);

                    da.Fill(item);

                    all = all.Concat(MethodRow(item));


                    con.Close();
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            TrackEvalutionPartListBox.ItemsSource = all.ToList();
        }

 



        private void TablesComboBox_Selected(object sender, SelectionChangedEventArgs e)
        {
            
            string str = ((ComboBoxItem)(TablesComboBox.SelectedItem)).Content.ToString();
            DataTable dtcb = ds.Tables[str];

            SqlDataAdapter da = new SqlDataAdapter("Select * from " + dtcb.TableName, con);

            dtcb.Rows.Clear();
            da.Fill(dtcb);

            List<MyT> temp = new List<MyT>();
          
            temp = MethodRow(dtcb);

            TrackEvalutionPartListBox.ItemsSource = temp;
        }

        private void TableStructButton_Click(object sender, RoutedEventArgs e)
        {
            if (TablesComboBox.SelectedIndex != -1)
            {
                string str = ((ComboBoxItem)(TablesComboBox.SelectedItem)).Content.ToString();
                DataTable dtcb = ds.Tables[str];

                List<MyT> strcts = new List<MyT>();

                foreach (DataColumn item in dtcb.Columns)
                {
                    MyT t = new MyT();
                    t.value = item.ColumnName + "\t\t\t\t" + item.DataType;
                    strcts.Add(t);
                }

                TrackEvalutionPartListBox.ItemsSource = strcts;
            }
            else
                MessageBox.Show("Выберите таблицу");
        }
    }
}
