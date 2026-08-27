// import { useState } from 'react';
// import { Link, useNavigate } from 'react-router-dom';
// import { useAuth } from '../context/AuthContext';
// import { useTheme } from '../context/ThemeContext';
// import Button from './ui/Button';

// export default function Navbar() {
//   const { isAuthenticated, isAdmin, user, logout } = useAuth();
//   const { theme, toggleTheme } = useTheme();
//   const navigate = useNavigate();
//   const [isMenuOpen, setIsMenuOpen] = useState(false);

//   function handleLogout() {
//     setIsMenuOpen(false);
//     logout();
//     navigate('/');
//   }

//   return (
//     <nav className="sticky top-0 z-20 border-b border-line bg-paper/90 backdrop-blur">
//       <div className="mx-auto flex max-w-5xl items-center justify-between px-4 py-4">
//         <Link
//           to="/"
//           onClick={() => setIsMenuOpen(false)}
//           className="flex items-center gap-2 font-display text-lg font-semibold text-ink"
//         >
//           <span className="flex h-7 w-7 items-center justify-center rounded-md bg-primary font-mono text-xs font-bold text-white">
//             EB
//           </span>
//           Event Booking
//         </Link>

//         {/* Desktop nav */}
//         <div className="hidden items-center gap-3 sm:flex">
//           <ThemeToggle theme={theme} onToggle={toggleTheme} />

//           {isAuthenticated ? (
//             <>
//               <span className="text-sm text-ink-soft">Hi, {user?.firstName}</span>
//               <Link to="/dashboard" className="text-sm font-medium text-ink-soft hover:text-ink">
//                 Dashboard
//               </Link>
//               {isAdmin && (
//                 <Link to="/admin" className="text-sm font-medium text-ink-soft hover:text-ink">
//                   Admin
//                 </Link>
//               )}
//               <Button variant="secondary" onClick={handleLogout}>
//                 Logout
//               </Button>
//             </>
//           ) : (
//             <>
//               <Link to="/login" className="text-sm font-medium text-ink-soft hover:text-ink">
//                 Log in
//               </Link>
//               <Link to="/register">
//                 <Button variant="primary">Register</Button>
//               </Link>
//             </>
//           )}
//         </div>

//         {/* Mobile controls */}
//         <div className="flex items-center gap-1 sm:hidden">
//           <ThemeToggle theme={theme} onToggle={toggleTheme} />
//           <button
//             type="button"
//             onClick={() => setIsMenuOpen((current) => !current)}
//             aria-label="Toggle menu"
//             aria-expanded={isMenuOpen}
//             className="flex h-9 w-9 items-center justify-center rounded-md text-ink-soft hover:bg-surface"
//           >
//             {isMenuOpen ? (
//               <svg viewBox="0 0 24 24" className="h-5 w-5" fill="none" stroke="currentColor" strokeWidth="2">
//                 <path strokeLinecap="round" d="M6 6l12 12M18 6L6 18" />
//               </svg>
//             ) : (
//               <svg viewBox="0 0 24 24" className="h-5 w-5" fill="none" stroke="currentColor" strokeWidth="2">
//                 <path strokeLinecap="round" d="M4 7h16M4 12h16M4 17h16" />
//               </svg>
//             )}
//           </button>
//         </div>
//       </div>

//       {/* Mobile menu panel */}
//       {isMenuOpen && (
//         <div className="border-t border-line bg-paper px-4 py-4 sm:hidden">
//           <div className="flex flex-col items-start gap-3">
//             {isAuthenticated ? (
//               <>
//                 <span className="text-sm text-ink-soft">Hi, {user?.firstName}</span>
//                 <Link to="/dashboard" onClick={() => setIsMenuOpen(false)} className="text-sm font-medium text-ink">
//                   Dashboard
//                 </Link>
//                 {isAdmin && (
//                   <Link to="/admin" onClick={() => setIsMenuOpen(false)} className="text-sm font-medium text-ink">
//                     Admin
//                   </Link>
//                 )}
//                 <Button variant="secondary" onClick={handleLogout} className="w-fit">
//                   Logout
//                 </Button>
//               </>
//             ) : (
//               <>
//                 <Link to="/login" onClick={() => setIsMenuOpen(false)} className="text-sm font-medium text-ink">
//                   Log in
//                 </Link>
//                 <Link to="/register" onClick={() => setIsMenuOpen(false)}>
//                   <Button variant="primary" className="w-fit">
//                     Register
//                   </Button>
//                 </Link>
//               </>
//             )}
//           </div>
//         </div>
//       )}
//     </nav>
//   );
// }

// function ThemeToggle({ theme, onToggle }) {
//   return (
//     <button
//       type="button"
//       onClick={onToggle}
//       aria-label="Toggle dark mode"
//       className="flex h-9 w-9 items-center justify-center rounded-md text-ink-soft transition hover:bg-surface hover:text-ink"
//     >
//       {theme === 'dark' ? (
//         <svg viewBox="0 0 24 24" className="h-5 w-5" fill="none" stroke="currentColor" strokeWidth="2">
//           <circle cx="12" cy="12" r="4" />
//           <path
//             strokeLinecap="round"
//             d="M12 2v2M12 20v2M4.9 4.9l1.4 1.4M17.7 17.7l1.4 1.4M2 12h2M20 12h2M4.9 19.1l1.4-1.4M17.7 6.3l1.4-1.4"
//           />
//         </svg>
//       ) : (
//         <svg viewBox="0 0 24 24" className="h-5 w-5" fill="currentColor">
//           <path d="M21 12.4A9 9 0 1111.6 3a7 7 0 009.4 9.4z" />
//         </svg>
//       )}
//     </button>
//   );
// }

import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useTheme } from '../context/ThemeContext';
import {
  CalendarDays,
  LayoutDashboard,
  LogOut,
  Menu,
  Moon,
  Shield,
  Sparkles,
  Sun,
  User,
  X,
} from 'lucide-react';
import Button from './ui/Button';

export default function Navbar() {
  const { isAuthenticated, isAdmin, user, logout } = useAuth();
  const { theme, toggleTheme } = useTheme();
  const navigate = useNavigate();
  const [isMenuOpen, setIsMenuOpen] = useState(false);

  function handleLogout() {
    setIsMenuOpen(false);
    logout();
    navigate('/');
  }

  return (
    <nav className="sticky top-0 z-20 border-b border-line bg-paper/80 backdrop-blur-xl">
      <div className="mx-auto flex max-w-5xl items-center justify-between px-4 py-3.5">
        <Link
          to="/"
          onClick={() => setIsMenuOpen(false)}
          className="group flex items-center gap-2.5 font-display text-lg font-semibold text-ink transition-colors hover:opacity-90"
        >
          <span className="flex h-8 w-8 items-center justify-center rounded-xl bg-gradient-to-br from-primary to-primary/70 shadow-lg shadow-primary/20 ring-1 ring-primary/30 transition-transform duration-300 group-hover:scale-105 group-hover:rotate-[-3deg]">
            <Sparkles className="h-4 w-4 text-white" strokeWidth={2.5} />
          </span>
          <span className="hidden sm:inline">Event Booking</span>
          <span className="sm:hidden">Events</span>
        </Link>

        {/* Desktop nav */}
        <div className="hidden items-center gap-1 sm:flex">
          <ThemeToggle onToggle={toggleTheme} theme={theme} />

          {isAuthenticated ? (
            <>
              <div className="mx-2 h-5 w-px bg-line" />

              <div className="flex items-center gap-2 rounded-full border border-line bg-surface/60 px-3 py-1.5">
                <User className="h-3.5 w-3.5 text-ink-soft" />
                <span className="text-sm font-medium text-ink">Hi, {user?.firstName}</span>
              </div>

              <NavLink to="/dashboard" icon={<LayoutDashboard className="h-3.5 w-3.5" />}>
                Dashboard
              </NavLink>

              {isAdmin && (
                <NavLink to="/admin" icon={<Shield className="h-3.5 w-3.5" />}>
                  Admin
                </NavLink>
              )}

              <Button
                onClick={handleLogout}
                variant="secondary"
                className="gap-1.5"
              >
                <LogOut className="h-3.5 w-3.5" />
                Logout
              </Button>
            </>
          ) : (
            <>
              <NavLink to="/events" icon={<CalendarDays className="h-3.5 w-3.5" />}>
                Events
              </NavLink>

              <Link
                to="/login"
                className="rounded-lg px-3 py-2 text-sm font-medium text-ink-soft transition hover:bg-surface hover:text-ink"
              >
                Log in
              </Link>

              <Link to="/register">
                <Button variant="primary">Register</Button>
              </Link>
            </>
          )}
        </div>

        {/* Mobile controls */}
        <div className="flex items-center gap-1 sm:hidden">
          <ThemeToggle onToggle={toggleTheme} theme={theme} />
          <button
            type="button"
            onClick={() => setIsMenuOpen((current) => !current)}
            aria-label="Toggle menu"
            aria-expanded={isMenuOpen}
            className="flex h-9 w-9 items-center justify-center rounded-lg text-ink-soft transition hover:bg-surface hover:text-ink"
          >
            {isMenuOpen ? (
              <X className="h-5 w-5" />
            ) : (
              <Menu className="h-5 w-5" />
            )}
          </button>
        </div>
      </div>

      {/* Mobile menu panel */}
      {isMenuOpen && (
        <div className="absolute inset-x-0 top-full border-b border-line bg-paper/95 px-4 pb-5 pt-3 shadow-xl shadow-ink/5 backdrop-blur-2xl sm:hidden">
          <div className="flex flex-col items-start gap-2">
            {isAuthenticated ? (
              <>
                <div className="mb-1 flex w-full items-center gap-3 rounded-xl border border-line bg-surface/50 px-3 py-2.5">
                  <span className="flex h-8 w-8 items-center justify-center rounded-full bg-primary/10 text-primary">
                    <User className="h-4 w-4" />
                  </span>
                  <div className="flex flex-col">
                    <span className="text-sm font-semibold text-ink">Hi, {user?.firstName}</span>
                    <span className="text-xs text-ink-soft">{user?.email}</span>
                  </div>
                </div>

                <MobileNavLink
                  to="/dashboard"
                  onClick={() => setIsMenuOpen(false)}
                  icon={<LayoutDashboard className="h-4 w-4" />}
                >
                  Dashboard
                </MobileNavLink>

                {isAdmin && (
                  <MobileNavLink
                    to="/admin"
                    onClick={() => setIsMenuOpen(false)}
                    icon={<Shield className="h-4 w-4" />}
                  >
                    Admin
                  </MobileNavLink>
                )}

                <button
                  type="button"
                  onClick={handleLogout}
                  className="mt-1 flex w-full items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium text-ink-soft transition hover:bg-surface hover:text-ink"
                >
                  <LogOut className="h-4 w-4" />
                  Logout
                </button>
              </>
            ) : (
              <>
                <MobileNavLink
                  to="/events"
                  onClick={() => setIsMenuOpen(false)}
                  icon={<CalendarDays className="h-4 w-4" />}
                >
                  Events
                </MobileNavLink>

                <MobileNavLink
                  to="/login"
                  onClick={() => setIsMenuOpen(false)}
                  icon={<User className="h-4 w-4" />}
                >
                  Log in
                </MobileNavLink>

                <Link
                  to="/register"
                  onClick={() => setIsMenuOpen(false)}
                  className="mt-1 w-full"
                >
                  <Button variant="primary" className="w-full">
                    Register
                  </Button>
                </Link>
              </>
            )}
          </div>
        </div>
      )}
    </nav>
  );
}

function ThemeToggle({ theme, onToggle }) {
  return (
    <button
      type="button"
      onClick={onToggle}
      aria-label="Toggle dark mode"
      className="flex h-9 w-9 items-center justify-center rounded-lg text-ink-soft transition hover:bg-surface hover:text-ink"
    >
      {theme === 'dark' ? (
        <Sun className="h-5 w-5" />
      ) : (
        <Moon className="h-5 w-5" />
      )}
    </button>
  );
}

function NavLink({ to, children, icon }) {
  return (
    <Link
      to={to}
      className="group flex items-center gap-1.5 rounded-lg px-3 py-2 text-sm font-medium text-ink-soft transition hover:bg-surface hover:text-ink"
    >
      <span className="transition-transform duration-200 group-hover:-translate-y-px">
        {icon}
      </span>
      {children}
    </Link>
  );
}

function MobileNavLink({ to, children, icon, onClick }) {
  return (
    <Link
      to={to}
      onClick={onClick}
      className="flex w-full items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium text-ink transition hover:bg-surface"
    >
      <span className="text-ink-soft">{icon}</span>
      {children}
    </Link>
  );
}
