using System;
using System.Linq;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stashbox.AspNet.SignalR;
using Stashbox.Infrastructure;

namespace Stashbox.SignalR.Tests
{
    [TestClass]
    public class SignalRExtensionTests
    {
        [TestMethod]
        public void ContainerExtensionTests_AddSignalR()
        {
            var config = new HubConfiguration();
            var container = new StashboxContainer().AddOwinSignalR(config);

            Assert.IsInstanceOfType(config.Resolver, typeof(StashboxDependencyResolver));
            Assert.IsTrue(container.CanResolve<Microsoft.AspNet.SignalR.IDependencyResolver>());
            Assert.IsTrue(container.CanResolve<IHubActivator>());
        }

        [TestMethod]
        public void ContainerExtensionTests_AddSignalR_WithAssembly()
        {
            var config = new HubConfiguration();
            var container = new StashboxContainer().AddOwinSignalR(config, typeof(TestHub).Assembly);

            var regs = container.ContainerContext.RegistrationRepository.GetAllRegistrations()
                .Where(reg => typeof(IHub).IsAssignableFrom(reg.ImplementationType) || typeof(PersistentConnection).IsAssignableFrom(reg.ImplementationType));

            Assert.IsTrue(container.CanResolve<TestHub>());
            Assert.IsTrue(container.CanResolve<Hub>());
            Assert.IsTrue(container.CanResolve<IHub>());
            Assert.IsTrue(container.CanResolve<TestConnection>());
            Assert.IsTrue(container.CanResolve<PersistentConnection>());
            Assert.IsTrue(regs.All(reg => !reg.ShouldHandleDisposal));
        }

        [TestMethod]
        public void ContainerExtensionTests_AddSignalR_WithTypes()
        {
            var config = new HubConfiguration();
            var container = new StashboxContainer().AddOwinSignalRWithTypes(config, typeof(TestHub), typeof(TestConnection));

            var regs = container.ContainerContext.RegistrationRepository.GetAllRegistrations()
                .Where(reg => typeof(IHub).IsAssignableFrom(reg.ImplementationType) || typeof(PersistentConnection).IsAssignableFrom(reg.ImplementationType));

            Assert.IsTrue(container.CanResolve<TestHub>());
            Assert.IsTrue(container.CanResolve<Hub>());
            Assert.IsTrue(container.CanResolve<IHub>());
            Assert.IsTrue(container.CanResolve<TestConnection>());
            Assert.IsTrue(container.CanResolve<PersistentConnection>());
            Assert.IsTrue(regs.All(reg => !reg.ShouldHandleDisposal));
        }

        [TestMethod]
        public void DependencyResolverTests_GetService_Null()
        {
            var config = new HubConfiguration();
            var container = new StashboxContainer().AddOwinSignalR(config);

            Assert.IsNull(container.Resolve<Microsoft.AspNet.SignalR.IDependencyResolver>().GetService(typeof(ITest)));
        }

        [TestMethod]
        public void DependencyResolverTests_GetServices_Null()
        {
            var config = new HubConfiguration();
            var container = new StashboxContainer().AddOwinSignalR(config);

            Assert.IsTrue(!container.Resolve<Microsoft.AspNet.SignalR.IDependencyResolver>().GetServices(typeof(ITest)).Any());
        }

        [TestMethod]
        public void DependencyResolverTests_GetService_PreferContainer()
        {
            var config = new HubConfiguration();
            var container = new StashboxContainer().AddOwinSignalR(config);
            container.RegisterType<ITest, Test>();
            container.Resolve<Microsoft.AspNet.SignalR.IDependencyResolver>().Register(typeof(ITest), () => new Test2());

            Assert.IsInstanceOfType(container.Resolve<Microsoft.AspNet.SignalR.IDependencyResolver>().GetService(typeof(ITest)), typeof(Test));
        }

        [TestMethod]
        public void DependencyResolverTests_GetServices_Concats_Container_And_DefaultResolver()
        {
            var config = new HubConfiguration();
            var container = new StashboxContainer().AddOwinSignalR(config);
            container.RegisterType<ITest, Test>();
            container.Resolve<Microsoft.AspNet.SignalR.IDependencyResolver>().Register(typeof(ITest), () => new Test2());

            var services = container.Resolve<Microsoft.AspNet.SignalR.IDependencyResolver>().GetServices(typeof(ITest));

            Assert.AreEqual(2, services.Count());
            Assert.IsInstanceOfType(services.First(), typeof(Test));
            Assert.IsInstanceOfType(services.Last(), typeof(Test2));
        }

        [TestMethod]
        public void DependencyResolverTests_GetService_NotDisposing()
        {
            var config = new HubConfiguration();
            TestHub hub;
            using (var container = new StashboxContainer())
            {
                container.RegisterType<ITest, Test>();
                container.AddOwinSignalR(config, typeof(TestHub).Assembly);
                hub = (TestHub)container.Resolve<Microsoft.AspNet.SignalR.IDependencyResolver>().GetService(typeof(TestHub));
            }

            Assert.IsFalse(hub.Disposed);
        }

        [TestMethod]
        public void HubActivatorTests_Create()
        {
            var config = new HubConfiguration();
            TestHub hub;
            using (var container = new StashboxContainer())
            {
                container.RegisterType<ITest, Test>();
                container.AddOwinSignalR(config, typeof(TestHub).Assembly);
                hub = (TestHub)container.Resolve<IHubActivator>().Create(new HubDescriptor { HubType = typeof(TestHub) });
            }

            Assert.IsFalse(hub.Disposed);
        }

        public interface ITest
        { }

        public class Test : ITest
        { }

        public class Test2 : ITest
        { }

        public class TestHub : Hub
        {
            public ITest Test { get; }

            public TestHub(ITest test)
            {
                this.Test = test;
            }

            public bool Disposed { get; private set; }

            protected override void Dispose(bool disposing)
            {
                if (this.Disposed)
                    throw new ObjectDisposedException("Test hub disposed.");

                this.Disposed = true;
                base.Dispose(disposing);
            }
        }

        public class TestConnection : PersistentConnection
        {
            public ITest Test { get; }

            public TestConnection(ITest test)
            {
                this.Test = test;
            }
        }
    }
}
