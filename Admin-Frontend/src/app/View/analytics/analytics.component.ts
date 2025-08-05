import { Component, ViewChild } from '@angular/core';
import { ChartConfiguration, ChartData, ChartType } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';
import { ProductStatisticResponse } from '../../Model/Analytic/ProductStatisticResponse';
import { AnalyticsService } from '../../Service/Analytic/analytics.service';
import { OrderByDate } from '../../Model/Analytic/OrderByDate';
import { finalize, forkJoin } from 'rxjs';

@Component({
  selector: 'app-analytics',
  standalone: false,
  templateUrl: './analytics.component.html',
  styleUrl: './analytics.component.css'
})
export class AnalyticsComponent {
  @ViewChild(BaseChartDirective) chart: BaseChartDirective | undefined;
  
  startDate: Date | null = null;
  endDate: Date | null = null;

  loading = false; // Biến trạng thái loading

  // Biến để kiểm tra có dữ liệu hay không
  hasOrderData = false;
  hasProductData = false;

  // Dữ liệu biểu đồ Line Chart
  public ordersLineChartData: ChartData<'line'> = { labels: [], datasets: [] };
  public ordersLineChartType: ChartType = 'line';

  // Dữ liệu biểu đồ Pie Chart
  public ordersPieChartData: ChartData<'pie'> = { labels: ['Đơn hàng thành công', 'Đơn hàng bị hủy'], datasets: [] };
  public ordersPieChartType: ChartType = 'pie';

  // Dữ liệu biểu đồ Bar Chart
  public productsBarChartData: ChartData<'bar'> = { labels: [], datasets: [] };
  public productsBarChartType: ChartType = 'bar';

public chartOptions: ChartConfiguration['options'] = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      position: 'top',
      labels: {
        font: {
          size: 16 // Kích thước phông chữ cho phần chú thích
        }
      }
    },
  },
  scales: {
    x: {
      ticks: {
        font: {
          size: 14 // Kích thước phông chữ cho nhãn trên trục X
        }
      }
    },
    y: {
      ticks: {
        font: {
          size: 14 // Kích thước phông chữ cho nhãn trên trục Y
        }
      }
    }
  }
};

  constructor(private analyticsService: AnalyticsService) { }

  ngOnInit() {
    this.initializeData();
  }

  initializeData() {
    this.endDate = new Date();
    this.startDate = new Date();
    this.startDate.setDate(this.startDate.getDate() - 7);
    this.fetchData();
  }

 fetchData() {
  if (!this.startDate || !this.endDate) {
    alert('Vui lòng chọn khoảng thời gian!');
    return;
  }

  this.loading = true;
  this.hasOrderData = false;
  this.hasProductData = false;

  forkJoin({
    orderData: this.analyticsService.getOrderByDateAnalytics(this.startDate, this.endDate),
    productData: this.analyticsService.getProductStatisticsAnalytics(5)
  }).pipe(
    finalize(() => this.loading = false)
  ).subscribe({
    next: (results) => {
      console.log(results);

      // --- Xử lý dữ liệu đơn hàng ---
      const orderResults = results.orderData;
      if (orderResults && orderResults.length > 0) {
        // Tổng hợp dữ liệu từ mảng
        const totalOrders = orderResults.reduce((sum, item) => sum + item.TotalOrders, 0);
        const totalRevenue = orderResults.reduce((sum, item) => sum + item.TotalRevenue, 0);
        const cancelledOrders = orderResults.reduce((sum, item) => sum + item.CancelledOrders, 0);
        
        // Tạo nhãn ngày tháng
        const labels = orderResults.map(item => new Date(item.Date).toLocaleDateString());
        const totalOrdersData = orderResults.map(item => item.TotalOrders);
        const totalRevenueData = orderResults.map(item => item.TotalRevenue);

        if (totalOrders > 0 || cancelledOrders > 0) {
          this.hasOrderData = true;
          this.ordersLineChartData = {
            labels: labels,
            datasets: [
              { label: 'Tổng doanh thu', data: totalRevenueData, borderColor: '#42A5F5', backgroundColor: 'rgba(66, 165, 245, 0.2)', fill: true },
              { label: 'Số lượng đơn hàng', data: totalOrdersData, borderColor: '#FFA726', backgroundColor: 'rgba(255, 167, 38, 0.2)', fill: true },
            ]
          };
          this.ordersPieChartData = {
            labels: ['Đơn hàng thành công', 'Đơn hàng bị hủy'],
            datasets: [{ data: [totalOrders - cancelledOrders, cancelledOrders], backgroundColor: ['#66BB6A', '#EF5350'] }]
          };
        } else {
          this.hasOrderData = false;
        }
      } else {
        this.hasOrderData = false;
      }

      // --- Xử lý dữ liệu sản phẩm ---
      const productResults = results.productData;
      if (productResults && productResults.length > 0) {
        this.hasProductData = true;
        this.productsBarChartData = {
          // Lấy nhãn và dữ liệu từ mảng
          labels: productResults.map(item => item.Product.Name),
          datasets: [{ 
            label: 'Số lượng bán', 
            data: productResults.map(item => item.TotalSales), 
            backgroundColor: ['#42A5F5'] 
          }]
        };
      } else {
        this.hasProductData = false;
      }

      this.chart?.update();
    },
    error: (err) => {
      console.error('Lỗi khi lấy dữ liệu phân tích:', err);
      this.hasOrderData = false;
      this.hasProductData = false;
    }
  });
}
}
