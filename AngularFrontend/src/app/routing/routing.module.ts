import { NgModule } from '@angular/core';
import {RouterModule, Routes} from "@angular/router";
import {UserComponent} from "../user/user.component";
import {AdminComponent} from "../admin/admin.component";

const routes: Routes = [
  {path: 'user', component : UserComponent, pathMatch: 'full'},
  {path: 'admin', component: AdminComponent, pathMatch: 'full'}
]

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class RoutingModule {}
export const routingComponents = [ UserComponent, AdminComponent]
