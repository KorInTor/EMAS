﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMAS_WPF.Windows;
using Service;
using Service.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EMAS_WPF;

namespace ViewModel
{
    public partial class TopMenuVM : ObservableObject
    {
        public Action DataSyncRequested;

        private IWindowsDialogueService dialogueService = new WindowsDialogueService();

        private PermissionInfo _currentUserPermissions = LocalSessionManager.PermissionInfo;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(OpenAdminMenuCommand))]
        private bool _isAdmin;

        public TopMenuVM()
        {
            _isAdmin = _currentUserPermissions.IsCurrentEmployeeAdmin;
        }

        [RelayCommand]
        private void SynchronizeData()
        {
            DataSyncRequested?.Invoke();
        }

        [RelayCommand]
        private void OpenAdminMenu()
        {
            dialogueService.ShowWindow<AdminWindow>();
        }

        [RelayCommand]
        private void OpenAboutWindow()
        {
            dialogueService.ShowSuccesfullMessage("\"Equipment Movement Accounting System - Система Отслеживания Передвижения Оборудования (СоПО)\"\r\n.Сделано студентами ТУСУР.\r\nПряхин Д.С.\r\nПетров А.В","О программе");
        }
    }
}
