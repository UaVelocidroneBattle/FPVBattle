import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import { BrowserRouter, Route, Routes, Navigate } from 'react-router-dom'
import MainLayout from './pages/layouts/MainLayout.tsx'
import RulesPage from './pages/RulesPage.tsx'
import StatisticsPage from './pages/statistics/StatisticsPage.tsx'
import DashboardPage from './pages/dashboard/DashboardPage.tsx'
import TracksPage from './pages/statistics/tracks/TracksPage.tsx'
import LeaderBoardPage from './pages/statistics/leaderboard/LeaderBoardPage.tsx'
import PilotsPage from './pages/statistics/pilots/PilotsPage.tsx'
import PilotProfilePage from './pages/statistics/pilot-profile/PilotProfilePage.tsx'


createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <BrowserRouter>
      <Routes>
        <Route path='/'>
          <Route element={<MainLayout />}>
            <Route index element={<DashboardPage />} />
            <Route path='rules' element={<RulesPage />} />
            <Route path='statistics' element={<StatisticsPage />} >
              <Route index element={<Navigate to="profile" replace />} />
              <Route path="profile" element={<PilotProfilePage />} />
              <Route path="leaderboard" element={<LeaderBoardPage />} />
              <Route path="tracks" element={<TracksPage />} />
              <Route path="pilots" element={<PilotsPage />} />
            </Route>
          </Route>
        </Route>
      </Routes>
    </BrowserRouter>
  </StrictMode>
)
