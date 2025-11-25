# 0. 安装dotnet trace
dotnet tool install --global dotnet-trace

## 内存快照分析
dotnet tool install --global dotnet-dump

## 实时性能计数器监控
dotnet tool install --global dotnet-counters

## 垃圾回收分析（可选）
dotnet tool install --global dotnet-gcdump

# 1. 启动程序并采集 30 秒
dotnet trace collect --profile cpu-sampling --duration 00:00:30 --output app.trace -- dotnet run

# 2. 转换为火焰图
dotnet trace convert app.trace --format speedscope -o app.speedscope.json

# 3. 打开 https://www.speedscope.app/ 上传查看

