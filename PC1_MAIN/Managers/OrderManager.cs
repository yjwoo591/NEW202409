using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Linq;
using ForexAITradingSystem.Models;

namespace ForexAITradingSystem.Managers
{

    public class OrderManager
    {
        private readonly ConcurrentQueue<Order> _pendingOrders = new();

        public async Task PlaceOrderAsync(Order order)
        {
            await Task.Run(() => _pendingOrders.Enqueue(order));
        }

        public async Task ProcessPendingOrdersAsync()
        {
            var tasks = new List<Task>();
            while (_pendingOrders.TryDequeue(out var order))
            {
                tasks.Add(ProcessOrderAsync(order));
            }
            await Task.WhenAll(tasks);
        }

        private async Task ProcessOrderAsync(Order order)
        {
            // 실제 주문 처리 로직
            await Task.Delay(100); // 시뮬레이션된 처리 시간
        }
    }
}