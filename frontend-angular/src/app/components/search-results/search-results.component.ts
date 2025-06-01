import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ModalAnalysisMediaSocialComponent } from '../modal-analysis-media-social/modal-analysis-media-social.component';

@Component({
  selector: 'app-search-results',
  standalone: true,
  imports: [CommonModule, ModalAnalysisMediaSocialComponent],
  templateUrl: './search-results.component.html',
})
export class SearchResultsComponent {
  @Input() searchResultsList!: any[];
  @Input() selectedSite!: string;
  @Input() searchResults!: any;
  @Input() isLoading!: boolean;
  @Input() analysisLink!: any;
  @Input() mainDataAnalysisLinkSocialVideo!: any;
  @Input() isShowModal!: boolean;
  @Input() isLoadingDataForModal!: boolean;
  @Output() selectSite = new EventEmitter<string>();
  @Output() analyzeLink = new EventEmitter<string>();
  @Output() analyzeVideo = new EventEmitter<string>();

  isResponseOfSiteHaveValue(site: string): boolean {
    return (
      this.searchResultsList.find((x) => x.siteSearch == site)?.showText == null
    );
  }

  isInSeviceSupport(link: string): boolean {
    const supportServices: string[] = ['tiktok.com', 'facebook.com'];
    return supportServices.some((ss) => link.includes(ss));
  }
}
