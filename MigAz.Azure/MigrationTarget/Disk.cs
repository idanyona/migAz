﻿using MigAz.Azure.Interface;
using MigAz.Core.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigAz.Azure.MigrationTarget
{
    public class Disk : IMigrationTarget
    {
        private string _TargetName = String.Empty;
        private Int32 _DiskSizeInGB = 0;
        private VirtualMachine _ParentVirtualMachine;

        private Disk() { }

        public Disk(Asm.Disk sourceDisk, VirtualMachine parentVirtualMachine)
        {
            this.SourceDisk = sourceDisk;
            this._ParentVirtualMachine = parentVirtualMachine;

            this.TargetName = sourceDisk.DiskName;
            this.Lun = sourceDisk.Lun;
            this.HostCaching = sourceDisk.HostCaching;
            this.DiskSizeInGB = sourceDisk.DiskSizeGb;
            this.TargetStorageAccountBlob = sourceDisk.StorageAccountBlob;
            this.SourceStorageAccount = sourceDisk.SourceStorageAccount;

        }

        public Disk(IArmDisk sourceDisk, VirtualMachine parentVirtualMachine)
        {
            this.SourceDisk = (IDisk)sourceDisk;
            this._ParentVirtualMachine = parentVirtualMachine;

            if (sourceDisk.GetType() == typeof(Azure.Arm.ClassicDisk))
            {
                Azure.Arm.ClassicDisk armDisk = (Azure.Arm.ClassicDisk)sourceDisk;

                this.TargetName = armDisk.Name;
                this.Lun = armDisk.Lun;
                this.HostCaching = armDisk.Caching;
                this.DiskSizeInGB = armDisk.DiskSizeGb;
                this.TargetStorageAccountBlob = armDisk.StorageAccountBlob;
                this.SourceStorageAccount = armDisk.SourceStorageAccount;
            }
            else if (sourceDisk.GetType() == typeof(Azure.Arm.ManagedDisk))
            {
                Azure.Arm.ManagedDisk armManagedDisk = (Azure.Arm.ManagedDisk)sourceDisk;

                this.TargetName = armManagedDisk.Name;
                this.DiskSizeInGB = armManagedDisk.DiskSizeGb;

                // todo now russell, so these seem to come from the VM itself, not the Managed Disk JSON?  ... or vice verse - review
                //this.Lun = armManagedDisk.Lun;
                //this.HostCaching = armManagedDisk.Caching;
                //this.TargetStorageAccountBlob = armManagedDisk.StorageAccountBlob;
            }
        }

        public VirtualMachine ParentVirtualMachine
        {
            get { return _ParentVirtualMachine; }
            set { _ParentVirtualMachine = value; }
        }

        public string TargetName
        {
            get { return _TargetName; }
            set { _TargetName = value.Trim().Replace(" ", String.Empty); }
        }

        public string HostCaching
        {
            get;set;
        }

        public IDisk SourceDisk { get; set; }

        public String SourceName
        {
            get
            {
                if (this.SourceDisk == null)
                    return String.Empty;
                else
                    return this.SourceDisk.ToString();
            }
        }

        public Int64? Lun
        {
            get;set;
        }

        public Int32 DiskSizeInGB
        {
            get { return _DiskSizeInGB; }
            set
            {
                if (value < 0)
                    throw new ArgumentException("DiskSizeInGB cannot be negative.");

                // https://azure.microsoft.com/en-us/blog/azure-introduces-new-disks-sizes-up-to-4tb/
                if (value > 4095)
                    throw new ArgumentException("DiskSizeInGB cannot be greater than 4095 GB.");
                
                _DiskSizeInGB = value;
            }
        }



        public IStorageAccount SourceStorageAccount
        {
            get; private set;
        }

        public IStorageTarget TargetStorageAccount
        {
            get;
            set;
        }
        
        public string TargetStorageAccountContainer
        {
            get { return "vhds"; }
        }

        public string TargetStorageAccountBlob
        {
            get; set;
        }

        public StorageAccountType StorageAccountType
        {
            get {
                if (this.TargetStorageAccount != null)
                    return this.TargetStorageAccount.StorageAccountType;

                return StorageAccountType.Premium; // TODO Russell
            }
        }

        public string TargetMediaLink
        {
            get
            {
                return "https://" + TargetStorageAccount.ToString() + "." + TargetStorageAccount.BlobStorageNamespace + "/vhds/" + this.TargetStorageAccountBlob;
            }
        }

        public bool IsManagedDisk
        {
            get
            {
                return this.TargetStorageAccount != null && this.TargetStorageAccount.GetType() == typeof(Azure.MigrationTarget.ManagedDiskStorage);
            }
        }

        public bool IsUnmanagedDisk
        {
            get
            {
                return this.TargetStorageAccount != null && (this.TargetStorageAccount.GetType() == typeof(Azure.MigrationTarget.StorageAccount) || this.TargetStorageAccount.GetType() == typeof(Azure.Arm.StorageAccount));
            }
        }
        public string ReferenceId
        {
            get
            {
                if (this.TargetStorageAccount == null)
                    return "[resourceId('Microsoft.Compute/disks/', '" + this.ToString() + "')]";
                else return 
                        String.Empty;
            }
        }

        public override string ToString()
        {
            return this.TargetName;
        }
    }
}
