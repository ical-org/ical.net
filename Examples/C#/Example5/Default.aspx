<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" Title="Untitled Page" %>
<asp:Content ID="Content1" ContentPlaceHolderID="Title" Runat="Server">
    DDay.iCal Example 5
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="Default" Runat="Server">
    <%-- Choose Calendars --%>
    <asp:Panel id="CalendarListPanel" CssClass="CalendarArea" runat="server">
        <div class="title">Choose Calendars</div>
        <div class="item">
            <asp:CheckBoxList ID="CalendarList" runat="server" AutoPostBack="true" />
        </div>
    </asp:Panel>
    
    <%-- Today's Events --%>
    <asp:Panel id="TodaysEventsPanel" CssClass="CalendarArea" runat="server">
        <div class="title">Today's Events</div>
        <asp:Repeater ID="TodaysEvents" runat="server">
            <ItemTemplate>
                <div class="item">
                    <div class="time"><%# GetTimeDisplay(Container.DataItem) %></div>
                    <span class="bold"><%# DataBinder.Eval(Container.DataItem, "Source.Summary") %></span>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </asp:Panel>
    
    <%-- Upcoming Events --%>
    <asp:Panel ID="UpcomingEventsPanel" runat="server" CssClass="CalendarArea">
        <div class="title">
            Upcoming Events</div>
        <asp:Repeater ID="UpcomingEvents" runat="server">
            <ItemTemplate>
                <div class="item">
                    <div class="date">
                        <%# ((DateTime)DataBinder.Eval(Container.DataItem, "Period.StartTime.Local")).ToString("ddd d - ") %>
                    </div>
                    <div class="time">
                        <%# GetTimeDisplay(Container.DataItem)%>
                    </div>
                    <span class="bold">
                        <%# DataBinder.Eval(Container.DataItem, "Source.Summary") %>
                    </span>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </asp:Panel>
    
    <asp:Panel ID="ConfigPanel" runat="server" CssClass="CalendarArea" Width="224px">
        <div class="item">Show 
            <asp:DropDownList ID="ddlDaysInFuture" runat="server" AutoPostBack="True">
                <asp:ListItem>3</asp:ListItem>
                <asp:ListItem>4</asp:ListItem>
                <asp:ListItem>5</asp:ListItem>
                <asp:ListItem>6</asp:ListItem>
                <asp:ListItem Selected="True">7</asp:ListItem>
                <asp:ListItem>8</asp:ListItem>
                <asp:ListItem>9</asp:ListItem>
                <asp:ListItem>10</asp:ListItem>
                <asp:ListItem>11</asp:ListItem>
                <asp:ListItem>12</asp:ListItem>
                <asp:ListItem>13</asp:ListItem>
                <asp:ListItem>14</asp:ListItem>
                <asp:ListItem>15</asp:ListItem>
                <asp:ListItem>16</asp:ListItem>
                <asp:ListItem>17</asp:ListItem>
                <asp:ListItem>18</asp:ListItem>
                <asp:ListItem>19</asp:ListItem>
                <asp:ListItem>20</asp:ListItem>
                <asp:ListItem>21</asp:ListItem>
                <asp:ListItem>22</asp:ListItem>
                <asp:ListItem>23</asp:ListItem>
                <asp:ListItem>24</asp:ListItem>
                <asp:ListItem>25</asp:ListItem>
                <asp:ListItem>26</asp:ListItem>
                <asp:ListItem>27</asp:ListItem>
                <asp:ListItem>28</asp:ListItem>
                <asp:ListItem>29</asp:ListItem>
                <asp:ListItem>30</asp:ListItem>
            </asp:DropDownList>
            Days in the Future</div>
    </asp:Panel>
</asp:Content>

