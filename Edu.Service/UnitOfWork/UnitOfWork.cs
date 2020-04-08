
using Edu.Data;
using Edu.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Edu.Service
{
    public partial class UnitOfWork : IDisposable
    {
	
		private DEduContext _DEduContext;
      
        public DEduContext DEduContext
        {
            get
            {
                if (this._DEduContext == null)
                {
                    this._DEduContext = new DEduContext(context);
                }
                return _DEduContext;
            }
        }

		
		private DImApply _DImApply;
      
        public DImApply DImApply
        {
            get
            {
                if (this._DImApply == null)
                {
                    this._DImApply = new DImApply(context);
                }
                return _DImApply;
            }
        }

		
		private DImFriendGroup _DImFriendGroup;
      
        public DImFriendGroup DImFriendGroup
        {
            get
            {
                if (this._DImFriendGroup == null)
                {
                    this._DImFriendGroup = new DImFriendGroup(context);
                }
                return _DImFriendGroup;
            }
        }

		
		private DImFriendGroupDetail _DImFriendGroupDetail;
      
        public DImFriendGroupDetail DImFriendGroupDetail
        {
            get
            {
                if (this._DImFriendGroupDetail == null)
                {
                    this._DImFriendGroupDetail = new DImFriendGroupDetail(context);
                }
                return _DImFriendGroupDetail;
            }
        }

		
		private DImGroup _DImGroup;
      
        public DImGroup DImGroup
        {
            get
            {
                if (this._DImGroup == null)
                {
                    this._DImGroup = new DImGroup(context);
                }
                return _DImGroup;
            }
        }

		
		private DImGroupDetail _DImGroupDetail;
      
        public DImGroupDetail DImGroupDetail
        {
            get
            {
                if (this._DImGroupDetail == null)
                {
                    this._DImGroupDetail = new DImGroupDetail(context);
                }
                return _DImGroupDetail;
            }
        }

		
		private DIMMsg _DIMMsg;
      
        public DIMMsg DIMMsg
        {
            get
            {
                if (this._DIMMsg == null)
                {
                    this._DIMMsg = new DIMMsg(context);
                }
                return _DIMMsg;
            }
        }

		
		private DLogInfo _DLogInfo;
      
        public DLogInfo DLogInfo
        {
            get
            {
                if (this._DLogInfo == null)
                {
                    this._DLogInfo = new DLogInfo(context);
                }
                return _DLogInfo;
            }
        }

		
		private DMenu _DMenu;
      
        public DMenu DMenu
        {
            get
            {
                if (this._DMenu == null)
                {
                    this._DMenu = new DMenu(context);
                }
                return _DMenu;
            }
        }

		
		private DUserDevice _DUserDevice;
      
        public DUserDevice DUserDevice
        {
            get
            {
                if (this._DUserDevice == null)
                {
                    this._DUserDevice = new DUserDevice(context);
                }
                return _DUserDevice;
            }
        }

		
		private DPlanTop _DPlanTop;
      
        public DPlanTop DPlanTop
        {
            get
            {
                if (this._DPlanTop == null)
                {
                    this._DPlanTop = new DPlanTop(context);
                }
                return _DPlanTop;
            }
        }

		
		private DPlan _DPlan;
      
        public DPlan DPlan
        {
            get
            {
                if (this._DPlan == null)
                {
                    this._DPlan = new DPlan(context);
                }
                return _DPlan;
            }
        }

		
		private DPlanDiscuss _DPlanDiscuss;
      
        public DPlanDiscuss DPlanDiscuss
        {
            get
            {
                if (this._DPlanDiscuss == null)
                {
                    this._DPlanDiscuss = new DPlanDiscuss(context);
                }
                return _DPlanDiscuss;
            }
        }

		
		private DPlanFile _DPlanFile;
      
        public DPlanFile DPlanFile
        {
            get
            {
                if (this._DPlanFile == null)
                {
                    this._DPlanFile = new DPlanFile(context);
                }
                return _DPlanFile;
            }
        }

		
		private DPlanProgress _DPlanProgress;
      
        public DPlanProgress DPlanProgress
        {
            get
            {
                if (this._DPlanProgress == null)
                {
                    this._DPlanProgress = new DPlanProgress(context);
                }
                return _DPlanProgress;
            }
        }

		
		private DUserInfo _DUserInfo;
      
        public DUserInfo DUserInfo
        {
            get
            {
                if (this._DUserInfo == null)
                {
                    this._DUserInfo = new DUserInfo(context);
                }
                return _DUserInfo;
            }
        }

		
		private DUserRole _DUserRole;
      
        public DUserRole DUserRole
        {
            get
            {
                if (this._DUserRole == null)
                {
                    this._DUserRole = new DUserRole(context);
                }
                return _DUserRole;
            }
        }

        private DConferenceMsg _DConferenceMsg;

        public DConferenceMsg DConferenceMsg
        {
            get
            {
                if (this._DConferenceMsg == null)
                {
                    this._DConferenceMsg = new DConferenceMsg(context);
                }
                return _DConferenceMsg;
            }
        }


        private DConferenceDiscuss _DConferenceDiscuss;

        public DConferenceDiscuss DConferenceDiscuss
        {
            get
            {
                if (this._DConferenceDiscuss == null)
                {
                    this._DConferenceDiscuss = new DConferenceDiscuss(context);
                }
                return _DConferenceDiscuss;
            }
        }


        private DRecentContacts _DRecentContacts;
      
        public DRecentContacts DRecentContacts
        {
            get
            {
                if (this._DRecentContacts == null)
                {
                    this._DRecentContacts = new DRecentContacts(context);
                }
                return _DRecentContacts;
            }
        }

		
		private DTopContacts _DTopContacts;
      
        public DTopContacts DTopContacts
        {
            get
            {
                if (this._DTopContacts == null)
                {
                    this._DTopContacts = new DTopContacts(context);
                }
                return _DTopContacts;
            }
        }

		
		private DTopContactsGroup _DTopContactsGroup;
      
        public DTopContactsGroup DTopContactsGroup
        {
            get
            {
                if (this._DTopContactsGroup == null)
                {
                    this._DTopContactsGroup = new DTopContactsGroup(context);
                }
                return _DTopContactsGroup;
            }
        }

        private DGroupAnnouncement _DGroupAnnouncement;

        public DGroupAnnouncement DGroupAnnouncement
        {
            get
            {
                if (this._DGroupAnnouncement == null)
                {
                    this._DGroupAnnouncement = new DGroupAnnouncement(context);
                }
                return _DGroupAnnouncement;
            }
        }
        private DGroupNotice _DGroupNotice;

        public DGroupNotice DGroupNotice
        {
            get
            {
                if (this._DGroupNotice == null)
                {
                    this._DGroupNotice = new DGroupNotice(context);
                }
                return _DGroupNotice;
            }
        }

        private DGroupSubject _DGroupSubject;

        public DGroupSubject DGroupSubject
        {
            get
            {
                if (this._DGroupSubject == null)
                {
                    this._DGroupSubject = new DGroupSubject(context);
                }
                return _DGroupSubject;
            }
        }

        private DUserNoticeSwitch _DUserNoticeSwitch;
        public DUserNoticeSwitch DUserNoticeSwitch
        {
            get
            {
                if (this._DUserNoticeSwitch == null)
                {
                    this._DUserNoticeSwitch = new DUserNoticeSwitch(context);
                }
                return _DUserNoticeSwitch;
            }
        }

        private DGetuiLog _DGetuiLog;

        public DGetuiLog DGetuiLog
        {
            get
            {
                if (this._DGetuiLog == null)
                {
                    this._DGetuiLog = new DGetuiLog(context);
                }
                return _DGetuiLog;
            }
        }

        private DUserFavorites _DUserFavorites;

        public DUserFavorites DUserFavorites
        {
            get
            {
                if (this._DUserFavorites == null)
                {
                    this._DUserFavorites = new DUserFavorites(context);
                }
                return _DUserFavorites;
            }
        }

    }
}