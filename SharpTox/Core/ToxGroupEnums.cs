namespace SharpTox.Core
{
    /// <summary>
    /// Privacy states a group can have.
    /// </summary>
    public enum ToxGroupPrivacyState
    {
        /// <summary>
        /// This group is public. Anyone can join the group using the chat ID.
        /// </summary>
        Public,

        /// <summary>
        /// This group is private. You can only be join this group via an invite from someone in your friend list.
        /// </summary>
        Private
    }

    /// <summary>
    /// Roles peers of a group can have.
    /// </summary>
    public enum ToxGroupRole
    {
        /// <summary>
        /// 
        /// </summary>
        Founder,

        /// <summary>
        /// 
        /// </summary>
        Moderator,

        /// <summary>
        /// 
        /// </summary>
        User,

        /// <summary>
        /// 
        /// </summary>
        Observer
    }

    public enum ToxErrorGroupNew
    {
        Ok,
        TooLong,
        Empty,
        Privacy,
        Init,
        State,
        Announce
    }

    public enum ToxErrorGroupJoin
    {
        Ok,
        Init,
        BadChatId,
        TooLong
    }

    public enum ToxErrorGroupReconnect
    {
        Ok,
        GroupNotFound
    }

    public enum ToxErrorGroupLeave
    {
        Ok,
        GroupNotFound,
        TooLong,
        FailSend,
        DeleteFail
    }

    public enum ToxErrorGroupSelfQuery
    {
        Ok,
        GroupNotFound
    }

    public enum ToxErrorGroupSelfNameSet
    {
        Ok,
        GroupNotFound,
        TooLong,
        Invalid,
        Taken,
        FailSend
    }

    public enum ToxErrorGroupSelfStatusSet
    {
        Ok,
        GroupNotFound,
        Invalid,
        FailSend
    }

    public enum ToxErrorGroupPeerQuery
    {
        Ok,
        GroupNotFound,
        PeerNotFound
    }

    public enum ToxErrorGroupStateQueries
    {
        Ok,
        GroupNotFound
    }

    public enum ToxErrorGroupTopicSet
    {
        Ok,
        GroupNotFound,
        TooLong,
        Permissions,
        FailCreate,
        FailSend
    }

    public enum ToxErrorGroupSendMessage
    {
        Ok,
        GroupNotFound,
        TooLong,
        Empty,
        BadType,
        Permissions,
        FailSend
    }

    public enum ToxErrorGroupSendPrivateMessage
    {
        Ok,
        GroupNotFound,
        PeerNotFound,
        TooLong,
        Empty,
        Permissions,
        FailSend
    }

    public enum ToxErrorGroupInviteFriend
    {
        Ok,
        GroupNotFound,
        FriendNotFound,
        InviteFail,
        FailSend
    }

    public enum ToxErrorGroupInviteAccept
    {
        Ok,
        BadInvite,
        InitFailed,
        TooLong
    }

    public enum ToxGroupJoinFail
    {
        NameTaken,
        PeerLimit,
        InvalidPassword,
        Unknown
    }

    public enum ToxErrorGroupFounderSetPassword
    {
        Ok,
        GroupNotFound,
        Permissions,
        TooLong,
        FailSend
    }

    public enum ToxErrorGroupFounderSetPrivacyState
    {
        Ok,
        GroupNotFound,
        Invalid,
        Permissions,
        FailSet,
        FailSend
    }

    public enum ToxErrorGroupFounderSetPeerLimit
    {
        Ok,
        GroupNotFound,
        Permissions,
        FailSet,
        FailSend
    }

    public enum ToxErrorGroupToggleIgnore
    {
        Ok,
        GroupNotFound,
        PeerNotFound
    }

    public enum ToxErrorGroupModSetRole
    {
        Ok,
        GroupNotFound,
        PeerNotFound,
        Permissions,
        Assignment,
        FailAction
    }

    public enum ToxErrorGroupModRemovePeer
    {
        Ok,
        GroupNotFound,
        PeerNotFound,
        Permissions,
        FailAction,
        FailSend
    }

    public enum ToxErrorGroupModRemoveBan
    {
        Ok,
        GroupNotFound,
        Permissions,
        FailAction,
        FailSend
    }

    public enum ToxGroupModEvent
    {
        Kick,
        Ban,
        Observer,
        User,
        Moderator
    }

    public enum ToxErrorGroupBanQuery
    {
        Ok,
        GroupNotFound,
        BadId
    }

    public enum ToxErrorGroupSendCustomPacket
    {
        Ok,
        GroupNotFound,
        TooLong,
        Empty,
        Permissions
    }
}
