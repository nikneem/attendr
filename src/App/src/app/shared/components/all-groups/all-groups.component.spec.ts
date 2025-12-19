import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AllGroupsComponent } from './all-groups.component';
import { AllGroupsStore } from '../../stores/all-groups.store';
import { AllGroupsService } from '../../services/all-groups.service';
import { provideHttpClient } from '@angular/common/http';

describe('AllGroupsComponent', () => {
    let component: AllGroupsComponent;
    let fixture: ComponentFixture<AllGroupsComponent>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [AllGroupsComponent],
            providers: [AllGroupsStore, AllGroupsService, provideHttpClient()],
        }).compileComponents();

        fixture = TestBed.createComponent(AllGroupsComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
