using System;

namespace WebFormsApp
{
    public partial class Default : System.Web.UI.Page
    {
        // ---- Часть 3. Шаг 6. Обработчики жизненного цикла страницы ----
        // Записи выводятся в Literal litLog в порядке их реального вызова.

        protected void Page_Init(object sender, EventArgs e)
        {
            litLog.Text += "Init<br/>";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            litLog.Text += "Load<br/>";

            // Шаг 8. Заполняем GridView только при первом заходе (не на постбеке),
            // чтобы наглядно показать рост __VIEWSTATE от табличных данных.
            if (!IsPostBack)
            {
                gvDemo.DataSource = new[]
                {
                    new { Id = 1, Name = "Первая строка", Value = 100 },
                    new { Id = 2, Name = "Вторая строка", Value = 200 },
                    new { Id = 3, Name = "Третья строка", Value = 300 }
                };
                gvDemo.DataBind();
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            litLog.Text += "PreRender<br/>";
        }

        protected void Page_Unload(object sender, EventArgs e)
        {
            // На экран вывести нельзя — страница уже отдана клиенту.
        }

        // ---- Часть 2. Шаг 4. Обработчик кнопки "Поздороваться" ----
        protected void btnOk_Click(object sender, EventArgs e)
        {
            lblResult.Text = "Привет, " + txtName.Text + "!";
        }

        // ---- Часть 4. Шаг 9. Кнопка "Очистить" ----
        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtName.Text = "";
            lblResult.Text = "";
        }

        // ---- Часть 4. Шаг 10. Мини-калькулятор с обработкой деления на ноль ----
        protected void btnCalc_Click(object sender, EventArgs e)
        {
            try
            {
                double a = double.Parse(txtA.Text);
                double b = double.Parse(txtB.Text);
                string op = ddlOp.SelectedValue;
                double r;

                switch (op)
                {
                    case "+": r = a + b; break;
                    case "-": r = a - b; break;
                    case "*": r = a * b; break;
                    case "/":
                        if (b == 0)
                            throw new DivideByZeroException();
                        r = a / b;
                        break;
                    default:
                        lblCalc.Text = "Неизвестная операция";
                        return;
                }

                lblCalc.Text = "Результат: " + r;
            }
            catch (DivideByZeroException)
            {
                lblCalc.Text = "Ошибка: деление на ноль недопустимо";
            }
            catch (FormatException)
            {
                lblCalc.Text = "Ошибка: введите корректные числа";
            }
        }
    }
}
