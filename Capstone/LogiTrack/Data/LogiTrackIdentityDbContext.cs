using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Data;

public sealed class LogiTrackIdentityDbContext(DbContextOptions<LogiTrackIdentityDbContext> options)
    : IdentityDbContext<IdentityUser>(options);