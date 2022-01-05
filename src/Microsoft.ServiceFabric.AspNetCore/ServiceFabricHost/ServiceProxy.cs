// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.Services.Communication.AspNetCore
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using Microsoft.Extensions.Hosting;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;

    internal static class ServiceProxy
    {
        private static readonly AssemblyBuilder AssemblyBuilder;
        private static readonly ModuleBuilder ModuleBuilder;

        static ServiceProxy()
        {
            AssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
                    new AssemblyName("Microsoft.ServiceFabric.Services.Communication.AspNetCore"),
                    AssemblyBuilderAccess.Run);

            ModuleBuilder = AssemblyBuilder.DefineDynamicModule("Root");
        }

        internal static Type CreateStatelessServiceProxy(Type baseServiceType)
        {
            TypeBuilder typeBuilder = ModuleBuilder.DefineType($"{baseServiceType.Name}_{Guid.NewGuid()}", TypeAttributes.Class, baseServiceType);

            // private IHost host;
            FieldBuilder hostFieldBuilder = typeBuilder.DefineField("host", typeof(IHost), FieldAttributes.Private);

            // Create constructor
            var ctor = GenerateConstructor(typeBuilder, hostFieldBuilder, baseServiceType);

            // Generate ServiceInstanceCreationAction method
            // (serviceContext) => new HostCommunicationListener(context, this.host);
            var actionBuilder = GenerateServiceInstanceCreationAction(typeBuilder, hostFieldBuilder);

            // Generate CreateHostServiceInstanceListener Method
            // new ServiceInstanceListener(ServiceInstanceAction);
            var hostServiceInstanceListenerMethod = GenerateCreateHostServiceInstanceListener(typeBuilder, actionBuilder);

            // Generate override IEnumerable<ServiceInstanceListener> IStatelessUserServiceInstance.CreateServiceInstanceListeners Method
            GenerateCreateServiceInstanceListeners(typeBuilder, baseServiceType, hostServiceInstanceListenerMethod);

            return typeBuilder.CreateType();
        }

        private static MethodBuilder GenerateCreateServiceInstanceListeners(TypeBuilder typeBuilder, Type baseServiceType, MethodBuilder hostServiceInstanceListenerMethod)
        {
            var method = typeBuilder.DefineMethod(
                "CreateServiceInstanceListeners",
                MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                CallingConventions.Standard,
                typeof(IEnumerable<ServiceInstanceListener>),
                null);
            var emitter = method.GetILGenerator();

            // Local variable -> List<ServiceInstanceListeners>
            var baseListenersList = emitter.DeclareLocal(typeof(List<ServiceInstanceListener>));

            // Local variable -> ServiceInstanceListener
            var hostServiceInstanceListener = emitter.DeclareLocal(typeof(ServiceInstanceListener));

            // base class method CreateServiceInstanceListeners method
            var baseListenerCreationMethod = baseServiceType.GetMethod("CreateServiceInstanceListeners");

            // Start method
            emitter.Emit(OpCodes.Nop);

            // call base class CreateServiceInstanceListeners method
            emitter.Emit(OpCodes.Ldarg_0);
            emitter.Emit(OpCodes.Call, baseListenerCreationMethod);

            // Convert to List<ServiceInstanceListener> and save
            emitter.Emit(OpCodes.Call, typeof(IEnumerable<ServiceInstanceListener>).GetMethod("ToList"));
            emitter.Emit(OpCodes.Stloc, baseListenersList);

            // call this.CreateHostServiceInstanceListener and save the hostServiceInstanceListener.
            emitter.Emit(OpCodes.Ldarg_0);
            emitter.Emit(OpCodes.Call, hostServiceInstanceListenerMethod);
            emitter.Emit(OpCodes.Stloc, hostServiceInstanceListener);

            // add hostServiceInstanceListener to listenersList.
            emitter.Emit(OpCodes.Ldloc, baseListenersList);
            emitter.Emit(OpCodes.Ldloc, hostServiceInstanceListener);
            emitter.Emit(OpCodes.Callvirt, typeof(List<ServiceInstanceListener>).GetMethod("Add"));

            emitter.Emit(OpCodes.Nop);

            // return listenerList
            emitter.Emit(OpCodes.Ldloc, baseListenersList);
            emitter.Emit(OpCodes.Ret);

            return method;
        }

        private static MethodBuilder GenerateCreateHostServiceInstanceListener(TypeBuilder typeBuilder, MethodBuilder serviceInstanceCeationActionBuilder)
        {
            var method = typeBuilder.DefineMethod(
                "CreateHostServiceInstanceListener",
                MethodAttributes.Private | MethodAttributes.HideBySig,
                CallingConventions.Standard,
                typeof(ServiceInstanceListener),
                null);

            var emitter = method.GetILGenerator();

            emitter.Emit(OpCodes.Nop);

            // load the ServiceInstanceCreationAction onto stack.
            emitter.Emit(OpCodes.Ldarg_0);
            emitter.Emit(OpCodes.Ldftn, serviceInstanceCeationActionBuilder);

            // create new ServiceInstanceListener.
            // new ServiceInstanceListener(ServiceInstanceCreationAction)
            emitter.Emit(OpCodes.Newobj, typeof(ServiceInstanceListener).GetConstructor(new Type[] { typeof(Action<StatelessServiceContext, ICommunicationListener>) }));
            emitter.Emit(OpCodes.Ldstr, string.Empty);

            // return
            emitter.Emit(OpCodes.Ret);

            return method;
        }

        private static MethodBuilder GenerateServiceInstanceCreationAction(TypeBuilder typeBuilder, FieldBuilder hostFieldBuilder)
        {
            var method = typeBuilder.DefineMethod(
                "ServiceInstanceCreationAction",
                MethodAttributes.Private | MethodAttributes.HideBySig,
                CallingConventions.Standard,
                typeof(ICommunicationListener),
                new Type[] { typeof(StatelessServiceContext) });
            var emitter = method.GetILGenerator();

            // local hostCommunicationListener
            // var hostCommunicationListener = emitter.DeclareLocal(typeof(HostCommunicationListener));
            emitter.Emit(OpCodes.Nop);

            // load service context onto stack
            emitter.Emit(OpCodes.Ldarg_1);

            // load this.host onto stack
            emitter.Emit(OpCodes.Ldarg_0);
            emitter.Emit(OpCodes.Ldfld, hostFieldBuilder);

            // new HostCommunicationListener(serviceContext, this.host)
            emitter.Emit(OpCodes.Newobj, typeof(HostCommunicationListener).GetConstructor(new Type[] { typeof(StatelessServiceContext), typeof(IHost) }));

            // save hostCommunicationListener
            // emitter.Emit(OpCodes.Stloc, hostCommunicationListener);
            // emitter.Emit(OpCodes.Ldloc, hostCommunicationListener);
            // return
            emitter.Emit(OpCodes.Ret);

            return method;
        }

        private static ConstructorBuilder GenerateConstructor(TypeBuilder typeBuilder, FieldBuilder hostFieldBuilder, Type baseType)
        {
            var baseCtor = baseType.GetConstructors(BindingFlags.Public)[0];

            // Get base service ctor paramerters.
            var parameters = baseCtor.GetParameters();
            var parameterTypes = parameters.Select(p => p.ParameterType).ToList();

            // Add IHost type in ctor.
            parameterTypes.Insert(0, typeof(IHost));

            // Define ctor and IL emmiter
            var ctor = typeBuilder.DefineConstructor(MethodAttributes.Public, baseCtor.CallingConvention, parameterTypes.ToArray());
            var emitter = ctor.GetILGenerator();

            emitter.Emit(OpCodes.Nop);

            // Load `this` and call base constructor with arguments
            // first argument is IHost
            emitter.Emit(OpCodes.Ldarg_0);
            for (var i = 2; i <= parameters.Length; ++i)
            {
                emitter.Emit(OpCodes.Ldarg, i);
            }

            emitter.Emit(OpCodes.Call, baseCtor);

            emitter.Emit(OpCodes.Nop);
            emitter.Emit(OpCodes.Nop);

            // save IHost
            // this.host = host;
            emitter.Emit(OpCodes.Ldarg_0);
            emitter.Emit(OpCodes.Ldarg_1);
            emitter.Emit(OpCodes.Stfld, hostFieldBuilder);

            // return
            emitter.Emit(OpCodes.Ret);

            return ctor;
        }
    }
}
