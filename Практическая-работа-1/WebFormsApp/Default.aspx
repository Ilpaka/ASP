<%@ Page Title="Главная" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="WebFormsApp.Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Часть 2. Форма с приветствием</h2>
    <div class="block">
        <asp:TextBox ID="txtName" runat="server" placeholder="Ваше имя" />
        <asp:Button ID="btnOk" runat="server" Text="Поздороваться" OnClick="btnOk_Click" />
        <asp:Button ID="btnClear" runat="server" Text="Очистить" OnClick="btnClear_Click" />
        <br />
        <asp:Label ID="lblResult" runat="server" CssClass="result" />
    </div>

    <h2>Часть 3. Жизненный цикл страницы</h2>
    <div class="block">
        <p>Порядок срабатывания событий при текущем запросе:</p>
        <asp:Literal ID="litLog" runat="server" />
    </div>

    <h2>Шаг 8. GridView (влияние на размер ViewState)</h2>
    <div class="block">
        <asp:GridView ID="gvDemo" runat="server" AutoGenerateColumns="true" />
    </div>

    <h2>Шаг 10. Мини-калькулятор</h2>
    <div class="block">
        <asp:TextBox ID="txtA" runat="server" placeholder="A" />
        <asp:DropDownList ID="ddlOp" runat="server">
            <asp:ListItem Text="+" Value="+" />
            <asp:ListItem Text="-" Value="-" />
            <asp:ListItem Text="*" Value="*" />
            <asp:ListItem Text="/" Value="/" />
        </asp:DropDownList>
        <asp:TextBox ID="txtB" runat="server" placeholder="B" />
        <asp:Button ID="btnCalc" runat="server" Text="Вычислить" OnClick="btnCalc_Click" />
        <br />
        <asp:Label ID="lblCalc" runat="server" CssClass="result" />
    </div>

</asp:Content>
