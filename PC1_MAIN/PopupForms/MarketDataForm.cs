using System;
using System.Windows.Forms;
using ForexAITradingSystem.Managers;

namespace ForexAITradingSystem.PopupForms
{
    public partial class MarketDataForm : Form
    {
        private readonly DataManager _dataManager;

        public MarketDataForm(DataManager dataManager)
        {
            InitializeComponent();
            _dataManager = dataManager;
        }

        // 여기에 필요한 메서드와 이벤트 핸들러를 추가하세요.
    }
}