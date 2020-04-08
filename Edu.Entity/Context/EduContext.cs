namespace Edu.Entity
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class EduContext : DbContext
    {
        public EduContext()
            : base("name=EduContext")
        {

        }
        public virtual DbSet<ImApply> ImApply { get; set; }
        public virtual DbSet<ImFriendGroup> ImFriendGroup { get; set; }
        public virtual DbSet<ImFriendGroupDetail> ImFriendGroupDetail { get; set; }
        public virtual DbSet<ImGroup> ImGroup { get; set; }
        public virtual DbSet<ImGroupDetail> ImGroupDetail { get; set; }
        public virtual DbSet<IMMsg> IMMsg { get; set; }
        public virtual DbSet<LogInfo> LogInfo { get; set; }
        public virtual DbSet<Menu> Menu { get; set; }
        public virtual DbSet<UserInfo> UserInfo { get; set; }
        public virtual DbSet<UserRole> UserRole { get; set; }
        public virtual DbSet<RecentContacts> RecentContacts { get; set; }
        public virtual DbSet<TopContacts> TopContacts { get; set; }
        public virtual DbSet<TopContactsGroup> TopContactsGroup { get; set; }
        public virtual DbSet<Plan> Plan { get; set; }
        public virtual DbSet<PlanProgress> PlanProgress { get; set; }
        public virtual DbSet<PlanFile> PlanFile { get; set; }
        public virtual DbSet<PlanDiscuss> PlanDiscuss { get; set; }
        public virtual DbSet<PlanTop> PlanTop { get; set; }
        public virtual DbSet<UserDevice> UserDevice { get; set; }
        public virtual DbSet<ConferenceMsg> ConferenceMsg { get; set; }
        public virtual DbSet<ConferenceDiscuss> ConferenceDiscuss { get; set; }
        public virtual DbSet<GroupAnnouncement> GroupAnnouncement { get; set; }
        public virtual DbSet<GroupNotice> GroupNotice { get; set; }
        public virtual DbSet<UserNoticeSwitch> UserNoticeSwitch { get; set; }
        public virtual DbSet<GroupSubject> GroupSubject { get; set; }
        public virtual DbSet<GetuiLog> GetuiLog { get; set; }
        public virtual DbSet<UserFavorites> UserFavorites { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
           
        }
    }
}
