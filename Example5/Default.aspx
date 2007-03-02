<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" Title="Untitled Page" %>
<asp:Content ID="Content1" ContentPlaceHolderID="Title" Runat="Server">
    DDay.iCal Example 5
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="Default" Runat="Server">
    <asp:Panel id="CalendarListPanel" CssClass="CalendarArea" runat="server">
        <div class="title">Choose Calendars</div>
        <div class="item">
            <asp:CheckBoxList ID="CalendarList" runat="server" AutoPostBack="true" />
        </div>
    </asp:Panel>
    <asp:Panel id="TodaysEventsPanel" CssClass="CalendarArea" runat="server">
        <div class="title">Today's Events</div>
        <asp:Repeater ID="TodaysEvents" runat="server">
            <ItemTemplate>
                <div class="item">
                    <div class="time"><%# ((DateTime)DataBinder.Eval(Container.DataItem, "Start.Local")).ToString("h:mm tt") %></div>
                    <span class="bold"><%# DataBinder.Eval(Container.DataItem, "Summary") %></span>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </asp:Panel>
    <asp:Panel ID="UpcomingEventsPanel" runat="server" CssClass="CalendarArea">
        <div class="title">
            Upcoming Events</div>
        <asp:Repeater ID="UpcomingEvents" runat="server">
            <ItemTemplate>
                <div class="item">
                    <div class="time">
                        <%# ((DateTime)DataBinder.Eval(Container.DataItem, "Start.Local")).ToString("d") %>
                    </div>
                    <div class="time">
                        <%# ((DateTime)DataBinder.Eval(Container.DataItem, "Start.Local")).ToString("h:mm tt") %>
                    </div>
                    <span class="bold">
                        <%# DataBinder.Eval(Container.DataItem, "Summary") %>
                    </span>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </asp:Panel>
</asp:Content>

