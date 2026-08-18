import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import { BrowserRouter, Route, Routes, Navigate } from 'react-router-dom'
import MainLayout from './pages/layouts/MainLayout.tsx'
import RulesLayout from './pages/guide/RulesLayout.tsx'
import GettingStartedPage from './pages/guide/GettingStartedPage.tsx'
import HowItWorksPage from './pages/guide/HowItWorksPage.tsx'
import DayStreakPage from './pages/guide/DayStreakPage.tsx'
import FreezePage from './pages/guide/FreezePage.tsx'
import QuadOfTheDayPage from './pages/guide/QuadOfTheDayPage.tsx'
import AchievementsPage from './pages/guide/AchievementsPage.tsx'
import GlobalRatingGuidePage from './pages/guide/GlobalRatingPage.tsx'
import LeaguesPage from './pages/guide/LeaguesPage.tsx'
import SupportPage from './pages/guide/SupportPage.tsx'
import StatisticsPage from './pages/statistics/StatisticsPage.tsx'
import CompetitionPage from './pages/competition/CompetitionPage.tsx'
import TracksPage from './pages/statistics/tracks/TracksPage.tsx'
import DayStreaksPage from './pages/statistics/daystreaks/DayStreaksPage.tsx'
import PilotsPage from './pages/statistics/pilots/PilotsPage.tsx'
import PilotProfilePage from './pages/statistics/pilot-profile/PilotProfilePage.tsx'
import GlobalRatingPage from './pages/statistics/global-rating/GlobalRatingPage.tsx'
import LandingPage from './pages/landing/LandingPage.tsx'
import ProfilePage from './pages/profile/ProfilePage.tsx'
import { registerAuthInterceptor } from './api/authInterceptor.ts'

registerAuthInterceptor()

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <BrowserRouter>
      <Routes>
        <Route path='/'>
          <Route element={<MainLayout />}>
            <Route index element={<LandingPage />} />
            <Route path='profile' element={<ProfilePage />} />
            <Route path='open' element={<CompetitionPage cupId="open-class" />} />
            <Route path='whoop' element={<CompetitionPage cupId="whoop-class" />} />
            <Route path='guide' element={<RulesLayout />}>
              <Route index element={<Navigate to="getting-started" replace />} />
              <Route path="getting-started" element={<GettingStartedPage />} />
              <Route path="how-it-works" element={<HowItWorksPage />} />
              <Route path="day-streak" element={<DayStreakPage />} />
              <Route path="freeze" element={<FreezePage />} />
              <Route path="quad-of-the-day" element={<QuadOfTheDayPage />} />
              <Route path="global-rating" element={<GlobalRatingGuidePage />} />
              <Route path="leagues" element={<LeaguesPage />} />
              <Route path="achievements" element={<AchievementsPage />} />
              <Route path="support" element={<SupportPage />} />
            </Route>
            <Route path='statistics' element={<StatisticsPage />} >
              <Route index element={<Navigate to="global-rating" replace />} />
              <Route path="global-rating" element={<GlobalRatingPage />} />
              <Route path="profile/:pilot?" element={<PilotProfilePage />} />
              <Route path="daystreaks" element={<DayStreaksPage />} />
              <Route path="tracks" element={<TracksPage />} />
              <Route path="pilots" element={<PilotsPage />} />
            </Route>
          </Route>
        </Route>
      </Routes>
    </BrowserRouter>
  </StrictMode>
)
