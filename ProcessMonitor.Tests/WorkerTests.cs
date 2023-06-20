using ProcessMonitor.Interface;
using ProcessMonitor.Worker;

namespace ProcessMonitor.Tests
{
    public class WorkerTests
    {
        private IWorker _worker;
        private IWorker _workerWithDefaults;

        [SetUp]
        public void Setup()
        {
            _workerWithDefaults = new ProcessMonitorWorker(true);
            _worker = new ProcessMonitorWorker();
        }
            

        [Test]
        public void ArgsValidation_Sucess()
        {
            string[] args = { "process", "1", "1" };

            var values = _worker.ValidateInputs(args);

            Assert.That("process", Is.EqualTo(values.ProcessName));
            Assert.That(1, Is.EqualTo(values.ProcessMaxLifetime));
            Assert.That(TimeSpan.FromMinutes(1), Is.EqualTo(values.MonitoringSpan));
        }

        [Test]
        public void ArgsValidation_ThrowsIndexOutOfRange_InvalidFormat()
        {
            string[] args = { "process", "abc" };

            Assert.Throws<FormatException>(() => _worker.ValidateInputs(args));
        }

        [Test]
        public void ArgsValidation_ThrowsIndexOutOfRange_InvalidIndex()
        {
            string[] args = { "process" };

            Assert.Throws<IndexOutOfRangeException>(() => _worker.ValidateInputs(args));
        }

        [Test]
        public void ArgsValidation_ThrowsIndexOutOfRange_SuccessDefaults()
        {
            string[] args = { "process" };
            var values = _workerWithDefaults.ValidateInputs(args);

            Assert.That("process", Is.EqualTo(values.ProcessName));
            Assert.That(5, Is.EqualTo(values.ProcessMaxLifetime));
            Assert.That(TimeSpan.FromMinutes(1), Is.EqualTo(values.MonitoringSpan));
        }
    }
}