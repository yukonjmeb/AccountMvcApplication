using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Helper
{
    public class ServiceFactory
    {
        /// <summary>
        /// 注入服務
        /// </summary>
        public static void InitInjector()
        {
            IUnityContainer unityContainer = new UnityContainer();

            RegisterType(unityContainer);
            var UnityLocator = new UnityServiceLocator(unityContainer);
            // 設定 Service Locator
            ServiceLocator.SetLocatorProvider(() => UnityLocator);

        }

        /// <summary>
        /// 註冊 Service & Repository
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static void RegisterType(IUnityContainer container)
        {
            // 注入 Repository
            container.RegisterType<IWebApiHelper, WebApiHelper>();
        }

        /// <summary>
        /// 取得服務實體
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetInstance<T>()
        {
            return ServiceLocator.Current.GetInstance<T>();
        }

    }
}
