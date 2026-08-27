// import { useState } from 'react';
// import { Link, useNavigate } from 'react-router-dom';
// import { useAuth } from '../context/AuthContext';
// import { ApiError } from '../api/client';
// import Input from '../components/ui/Input';
// import Button from '../components/ui/Button';
// import { ErrorAlert } from '../components/ui/Alert';

// export default function RegisterPage() {
//   const { register } = useAuth();
//   const navigate = useNavigate();

//   const [firstName, setFirstName] = useState('');
//   const [lastName, setLastName] = useState('');
//   const [email, setEmail] = useState('');
//   const [password, setPassword] = useState('');
//   const [confirmPassword, setConfirmPassword] = useState('');
//   const [isSubmitting, setIsSubmitting] = useState(false);
//   const [errorMessage, setErrorMessage] = useState(null);
//   const [errorList, setErrorList] = useState(null);

//   async function handleSubmit(event) {
//     event.preventDefault();
//     setErrorMessage(null);
//     setErrorList(null);

//     if (password !== confirmPassword) {
//       setErrorMessage('Passwords do not match.');
//       return;
//     }

//     setIsSubmitting(true);

//     try {
//       await register(firstName, lastName, email, password);
//       navigate('/', { replace: true });
//     } catch (error) {
//       if (error instanceof ApiError) {
//         if (Array.isArray(error.errors) && error.errors.length > 0) {
//           setErrorList(error.errors);
//         } else {
//           setErrorMessage(error.message);
//         }
//       } else {
//         setErrorMessage('Something went wrong. Please try again.');
//       }
//     } finally {
//       setIsSubmitting(false);
//     }
//   }

//   return (
//     <div className="flex min-h-[calc(100vh-73px)] items-center justify-center px-4 py-12">
//       <div className="w-full max-w-sm overflow-hidden rounded-2xl border border-line bg-paper shadow-sm">
//         <div className="h-1.5 w-full bg-accent" />

//         <div className="p-7">
//           <p className="font-mono text-xs uppercase tracking-wide text-ink-faint">Get started</p>
//           <h1 className="mt-1 font-display text-xl font-semibold text-ink">Create your account</h1>

//           <form onSubmit={handleSubmit} className="mt-6 flex flex-col gap-4">
//             <div className="grid grid-cols-2 gap-3">
//               <Input
//                 label="First name"
//                 id="firstName"
//                 type="text"
//                 value={firstName}
//                 onChange={(event) => setFirstName(event.target.value)}
//                 required
//               />
//               <Input
//                 label="Last name"
//                 id="lastName"
//                 type="text"
//                 value={lastName}
//                 onChange={(event) => setLastName(event.target.value)}
//                 required
//               />
//             </div>

//             <Input
//               label="Email"
//               id="email"
//               type="email"
//               value={email}
//               onChange={(event) => setEmail(event.target.value)}
//               required
//             />

//             <Input
//               label="Password"
//               id="password"
//               type="password"
//               value={password}
//               onChange={(event) => setPassword(event.target.value)}
//               required
//             />

//             <Input
//               label="Confirm password"
//               id="confirmPassword"
//               type="password"
//               value={confirmPassword}
//               onChange={(event) => setConfirmPassword(event.target.value)}
//               required
//             />

//             <ErrorAlert message={errorMessage} list={errorList} />

//             <Button type="submit" disabled={isSubmitting} className="mt-1 w-full">
//               {isSubmitting ? 'Registering...' : 'Register'}
//             </Button>
//           </form>

//           <p className="mt-6 text-center text-sm text-ink-soft">
//             Already have an account?{' '}
//             <Link to="/login" className="font-medium text-primary hover:underline">
//               Log in
//             </Link>
//           </p>
//         </div>
//       </div>
//     </div>
//   );
// }

import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { UserPlus } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import { ApiError } from '../api/client';
import Input from '../components/ui/Input';
import Button from '../components/ui/Button';
import { ErrorAlert } from '../components/ui/Alert';

export default function RegisterPage() {
  const { register } = useAuth();
  const navigate = useNavigate();

  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState(null);
  const [errorList, setErrorList] = useState(null);

  async function handleSubmit(event) {
    event.preventDefault();
    setErrorMessage(null);
    setErrorList(null);

    if (password !== confirmPassword) {
      setErrorMessage('Passwords do not match.');
      return;
    }

    setIsSubmitting(true);

    try {
      await register(firstName, lastName, email, password);
      navigate('/', { replace: true });
    } catch (error) {
      if (error instanceof ApiError) {
        if (Array.isArray(error.errors) && error.errors.length > 0) {
          setErrorList(error.errors);
        } else {
          setErrorMessage(error.message);
        }
      } else {
        setErrorMessage('Something went wrong. Please try again.');
      }
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="relative flex min-h-[calc(100vh-73px)] items-center justify-center overflow-hidden px-4 py-12">
      <div className="pointer-events-none absolute inset-0 -z-10 bg-[radial-gradient(ellipse_at_top,var(--color-accent)_0%,transparent_55%)] opacity-[0.07]" />
      <div className="pointer-events-none absolute inset-0 -z-10 bg-surface/40 backdrop-blur-3xl" />

      <div className="w-full max-w-md">
        <div className="overflow-hidden rounded-2xl border border-line bg-paper/80 backdrop-blur-xl shadow-xl shadow-ink/5">
          <div className="flex flex-col items-center p-8">
            <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-accent/10 text-accent ring-1 ring-accent/20">
              <UserPlus className="h-6 w-6" strokeWidth={2} />
            </div>

            <p className="mt-6 font-mono text-[11px] font-semibold uppercase tracking-[0.14em] text-ink-soft">
              Get started
            </p>
            <h1 className="mt-1.5 text-center font-display text-xl font-semibold tracking-tight text-ink">
              Create your account
            </h1>

            <form onSubmit={handleSubmit} className="mt-7 w-full space-y-4">
              <div className="grid grid-cols-2 gap-3">
                <Input
                  label="First name"
                  id="firstName"
                  type="text"
                  value={firstName}
                  onChange={(event) => setFirstName(event.target.value)}
                  required
                />
                <Input
                  label="Last name"
                  id="lastName"
                  type="text"
                  value={lastName}
                  onChange={(event) => setLastName(event.target.value)}
                  required
                />
              </div>

              <Input
                label="Email"
                id="email"
                type="email"
                value={email}
                onChange={(event) => setEmail(event.target.value)}
                required
              />

              <Input
                label="Password"
                id="password"
                type="password"
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                required
              />

              <Input
                label="Confirm password"
                id="confirmPassword"
                type="password"
                value={confirmPassword}
                onChange={(event) => setConfirmPassword(event.target.value)}
                required
              />

              <ErrorAlert message={errorMessage} list={errorList} />

              <Button
                type="submit"
                disabled={isSubmitting}
                className="w-full transition-all duration-200 active:scale-[0.98] hover:opacity-90 disabled:cursor-not-allowed disabled:opacity-60"
              >
                {isSubmitting ? (
                  <span className="flex items-center justify-center gap-2">
                    <span className="h-4 w-4 animate-spin rounded-full border-2 border-current border-t-transparent" />
                    Registering...
                  </span>
                ) : (
                  'Register'
                )}
              </Button>
            </form>

            <p className="mt-6 text-center text-sm text-ink-soft">
              Already have an account?{' '}
              <Link
                to="/login"
                className="font-medium text-primary transition-colors hover:text-primary/80 hover:underline"
              >
                Log in
              </Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
