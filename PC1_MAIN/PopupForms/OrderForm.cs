using System;
using System.Windows.Forms;
using ForexAITradingSystem.Models;
using ForexAITradingSystem.Managers;

namespace ForexAITradingSystem.PopupForms
{
    public partial class OrderForm : Form
    {
        private readonly OrderManager _orderManager;
        public Order? Order { get; private set; }

        public OrderForm(OrderManager orderManager)
        {
            InitializeComponent();
            _orderManager = orderManager;
        }

        // 여기에 다른 메서드들을 추가하세요...
    }
}