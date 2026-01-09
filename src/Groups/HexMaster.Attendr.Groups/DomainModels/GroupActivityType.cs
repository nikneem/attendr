// This file is kept for backward compatibility during refactoring.
// The actual GroupActivityType class is now defined in HexMaster.Attendr.Groups.Abstractions.DomainModels.
// This file simply re-exports it to avoid breaking existing usages.

using HexMaster.Attendr.Groups.Abstractions.DomainModels;

namespace HexMaster.Attendr.Groups.DomainModels
{
    // Type aliases to maintain compatibility
    using GroupActivityType = HexMaster.Attendr.Groups.Abstractions.DomainModels.GroupActivityType;
    using ActivitySeverity = HexMaster.Attendr.Groups.Abstractions.DomainModels.ActivitySeverity;
    using GroupActivityTypeProfileJoinedGroup = HexMaster.Attendr.Groups.Abstractions.DomainModels.GroupActivityTypeProfileJoinedGroup;
    using GroupActivityTypeProfileLeftGroup = HexMaster.Attendr.Groups.Abstractions.DomainModels.GroupActivityTypeProfileLeftGroup;
    using GroupActivityTypeProfilePresentationCheckedIn = HexMaster.Attendr.Groups.Abstractions.DomainModels.GroupActivityTypeProfilePresentationCheckedIn;
    using GroupActivityTypeProfilePresentationCheckedOut = HexMaster.Attendr.Groups.Abstractions.DomainModels.GroupActivityTypeProfilePresentationCheckedOut;
    using GroupActivityTypeProfileAttendingConference = HexMaster.Attendr.Groups.Abstractions.DomainModels.GroupActivityTypeProfileAttendingConference;
    using GroupActivityTypeProfileLeavingConference = HexMaster.Attendr.Groups.Abstractions.DomainModels.GroupActivityTypeProfileLeavingConference;
}
