﻿using System;
using System.Collections.Generic;
using System.Windows.Input;
using DevelopmentInProgress.DipState;
using DevelopmentInProgress.Origin.Messages;
using DevelopmentInProgress.ExampleModule.Model;
using DevelopmentInProgress.ExampleModule.Service;
using DevelopmentInProgress.Origin.Context;
using DevelopmentInProgress.Origin.ViewModel;

namespace DevelopmentInProgress.ExampleModule.ViewModel
{
    public class CustomerRemediationViewModel : DocumentViewModel
    {
        private readonly RemediationService remediationService;

        public CustomerRemediationViewModel(ViewModelContext viewModelContext, RemediationService remediationService)
            : base(viewModelContext)
        {
            this.remediationService = remediationService;
            CompleteCommand = new ViewModelCommand(Complete);
            FailCommand = new ViewModelCommand(Fail);

            Products = new List<string>() {"Cap", "Collar", "Structured Collar"};
        }

        public ICommand CompleteCommand { get; set; }
        public ICommand FailCommand { get; set; }
        public List<Customer> Customers { get; set; }
        public Customer CurrentCustomer { get; set; }
        public List<string> Products { get; set; }

        protected override ProcessAsyncResult OnPublishedAsync()
        {
            GetCustomersAsync();
            return base.OnPublishedAsync();
        }

        private async void GetCustomersAsync()
        {
            try
            {
                Customers = await remediationService.GetCustomersAsync();
            }
            catch (StateException ex)
            {
                ex.Messages.ForEach(
                    m => ShowMessage(new Message() {MessageType = MessageTypeEnum.Error, Text = m}, true));
            }
        }

        private async void Complete(object param)
        {
            ClearMessages();

            var state = param as State;
            try
            {
                IsBusy = true;
                await state.ExecuteAsync(StateStatus.Complete);
            }
            catch (StateException ex)
            {
                ex.Messages.ForEach(
                    m => ShowMessage(new Message() {MessageType = MessageTypeEnum.Warn, Text = m}, true));
            }
            finally
            {
                var entityBase = param as EntityBase;
                entityBase.OnPropertyChanged(String.Empty);
                CurrentCustomer.OnPropertyChanged(String.Empty);
                IsBusy = false;
            }
        }

        private void Fail(object param)
        {
            var state = param as State;
            remediationService.Run(state, StateStatus.Fail);

            CurrentCustomer.OnPropertyChanged("RemediationWorkflow");
        }
    }
}
